using Microsoft.AspNetCore.Mvc;

namespace WebApplication4.Controllers;
public class LoginController : Controller
{
    public IActionResult ValidateLogin()
    {
        return View();
    }
}
