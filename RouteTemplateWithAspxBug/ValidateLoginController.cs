using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers;
public class ValidateLoginController : Controller
{
    public IActionResult ValidateLogin()
    {
        return View();
    }
}
