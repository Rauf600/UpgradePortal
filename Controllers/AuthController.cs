using Microsoft.AspNetCore.Mvc;
using UpgradePortal.Web.Services;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await _authService.ValidateUserAsync(model.Email, model.Password);

        if (user == null)
        {
            ViewBag.Error = "Invalid email or password";
            return View(model);
        }

        await _authService.SignInAsync(HttpContext, user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync(HttpContext);
        return RedirectToAction("Login");
    }
}