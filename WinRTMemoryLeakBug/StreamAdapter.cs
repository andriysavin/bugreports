using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage.Streams;

internal class StreamAdapter : IRandomAccessStream
{
    private readonly Stream stream;

    public StreamAdapter(Stream innerStream)
    {
        stream = innerStream;
    }

    public bool CanRead => stream.CanRead;
    public bool CanWrite => stream.CanWrite;
    public ulong Position => (ulong)stream.Position;
    public ulong Size { get => (ulong)stream.Length; set => stream.SetLength((long)value); }

    public void Seek(ulong position) => stream.Seek((long)position, SeekOrigin.Begin);
    public IRandomAccessStream CloneStream() => throw new NotSupportedException();

    public IAsyncOperation<bool> FlushAsync()
    {
        return AsyncInfo.Run((cancelToken) => Task.FromResult(true));
    }

    public void Dispose() => stream.Dispose();

    public IInputStream GetInputStreamAt(ulong position)
        => throw new NotSupportedException();

    public IOutputStream GetOutputStreamAt(ulong position)
        => throw new NotSupportedException();

    public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
    {
        throw new NotSupportedException();
    }

    public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
    {
        // I suspect this buffer is not being released for some reason,
        // and more and more buffers are being allocated.
        // This no-op code is here just to ensure that it's NOT the
        // Stream adapter who continues keeping references to
        // all buffer instances, preventing them from being released.
        uint bytesToWrite = buffer.Length;

        return AsyncInfo.Run<uint, uint>(
            (cancelToken, progressListener) => Task.FromResult(bytesToWrite));
    }
}
