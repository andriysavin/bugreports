using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;

string sourceFilePath = Path.GetFullPath(@"source.wav");
string destinationFilePath = Path.GetFullPath(@"transcoded.mp3");

while (true)
{
    await TranscodeFile(sourceFilePath, destinationFilePath);

    File.Delete(destinationFilePath);
}

static async Task TranscodeFile(
    string sourceFilePath,
    string destinationFilePath)
{
    // INSTRUCTIONS: Try different destination file stream
    // opening approaches by uncommenting only one at time.
    // Run in VS debugger and observe memory graph in the Diagnostic Tools
    // panel.

    // This is using .Net's  FileStream with the WinRT adapter.
    // Results in memory leak.
    using var dstStream = OpenDestinationFileDotNet(destinationFilePath);

    // This is similar to previous, but uses custom adapter which does no-op,
    // and still results in memory leak.
    //using var dstStream = OpenDestinationFileDotNetWithCustomAdapter(destinationFilePath);
    
    // This uses native WinRT stream and does NOT result in memory leak.
    //using var dstStream = await OpenDestinationFileWinRt(destinationFilePath);


    // Source file is opened as native WinRT stream,
    // but seems to be not triggering memory leak
    // even if .Net's stream is used.
    using var srcStream = await OpenSourceFile(sourceFilePath);

    await TranscodeStream(srcStream, dstStream);

    // Instead of transcoding, you can also try to
    // just copy streams using the WinRT API,
    // which, from the perspective of using streams seems
    // very similar to transcoding. Yet, it doesn't result
    // in the memory leak. It may have something to do with how
    // buffers are used - copying is probably allocating just
    // a single WinRT buffer, while MediaTranscoder seems to
    // be allocating many of them, and also not releasing them
    // for some reason.
    //await RandomAccessStream.CopyAsync(srcStream, dstStream);
}

static async Task TranscodeStream(IRandomAccessStream srcStream, IRandomAccessStream dstStream)
{
    var transcoder = new MediaTranscoder();
    var encodingProfile = CreateEncodingProfile();

    var prepareOp = await transcoder.PrepareStreamTranscodeAsync(
        srcStream,
        dstStream,
        encodingProfile);

    if (!prepareOp.CanTranscode)
    {
        throw new InvalidOperationException("Can't transcode");
    }

    await prepareOp.TranscodeAsync();
}

static IRandomAccessStream OpenDestinationFileDotNet(string filePath)
{
    var dstNetStream = File.OpenWrite(filePath);
    return dstNetStream.AsRandomAccessStream();
}

static IRandomAccessStream OpenDestinationFileDotNetWithCustomAdapter(string filePath)
{
    var dstNetStream = File.OpenWrite(filePath);
    return new StreamAdapter(dstNetStream);
}

static async Task<IRandomAccessStream> OpenDestinationFileWinRt(string filePath)
{
    var outputDir = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));

    var destinationFile = await outputDir.CreateFileAsync(
        Path.GetFileName(filePath),
        CreationCollisionOption.ReplaceExisting);

    return await destinationFile.OpenAsync(FileAccessMode.ReadWrite);
}

static async Task<IRandomAccessStream> OpenSourceFile(string filePath)
{
    var sourceFile = await StorageFile.GetFileFromPathAsync(filePath);
    return await sourceFile.OpenAsync(FileAccessMode.Read);
}

static MediaEncodingProfile CreateEncodingProfile()
{
    return new MediaEncodingProfile
    {
        Audio = new AudioEncodingProperties
        {
            Bitrate = 64000,
            BitsPerSample = 16,
            SampleRate = 44100,
            ChannelCount = 1,
            Subtype = "MP3",
        },
        Container = new ContainerEncodingProperties
        {
            Subtype = "MP3"
        }
    };
}

static async Task<StorageFile> CreateDestinationFile(string dstFilePath)
{
    var outputDir = await StorageFolder.GetFolderFromPathAsync(
        Path.GetDirectoryName(dstFilePath));

    var destinationFile = await outputDir.CreateFileAsync(
        Path.GetFileName(dstFilePath),
        CreationCollisionOption.ReplaceExisting);

    return destinationFile;
}

