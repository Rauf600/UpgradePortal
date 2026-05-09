using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Services;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly SendGridEmailService _emailService;
    private readonly AppDbContext _db;

    public AuthController(
        AuthService authService,
        SendGridEmailService emailService,
        AppDbContext db)
    {
        _authService = authService;
        _emailService = emailService;
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _authService.ValidateUserAsync(model.Email, model.Password);

        if (user == null)
        {
            ViewBag.Error = "Invalid email or password";
            return View(model);
        }

        if (user.TwoFactorEnabled)
        {
            var code = AuthService.GenerateSixDigitCode();

            HttpContext.Session.SetString("2fa_email", user.Email);
            HttpContext.Session.SetString("2fa_code", code);

            var sent = await _emailService.SendTwoFactorCodeAsync(user.Email, code);

            if (!sent)
            {
                ViewBag.Error = "Unable to send the verification code. Check SendGrid settings.";
                return View(model);
            }

            return RedirectToAction(nameof(TwoFactor));
        }

        await _authService.SignInAsync(HttpContext, user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult TwoFactor()
    {
        var email = HttpContext.Session.GetString("2fa_email");
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(Login));

        return View(new TwoFactorViewModel { Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
    {
        var savedEmail = HttpContext.Session.GetString("2fa_email");
        var savedCode = HttpContext.Session.GetString("2fa_code");

        if (string.IsNullOrWhiteSpace(savedEmail) || string.IsNullOrWhiteSpace(savedCode))
            return RedirectToAction(nameof(Login));

        if (model.Code != savedCode)
        {
            ViewBag.Error = "Invalid verification code";
            model.Email = savedEmail;
            return View(model);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == savedEmail && u.IsActive);

        if (user == null)
            return RedirectToAction(nameof(Login));

        HttpContext.Session.Remove("2fa_email");
        HttpContext.Session.Remove("2fa_code");

        await _authService.SignInAsync(HttpContext, user);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == model.Email && x.IsActive);

        if (user == null)
        {
            ViewBag.Message = "If the email exists, a reset code has been sent.";
            return View();
        }

        var code = AuthService.GenerateSixDigitCode();

        HttpContext.Session.SetString("reset_email", user.Email);
        HttpContext.Session.SetString("reset_code", code);

        var sent = await _emailService.SendPasswordResetCodeAsync(user.Email, code);

        if (!sent)
        {
            ViewBag.Error = "Unable to send reset code. Check SendGrid settings.";
            return View(model);
        }

        return RedirectToAction(nameof(ResetPassword));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword()
    {
        var email = HttpContext.Session.GetString("reset_email");
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(ForgotPassword));

        return View(new ResetPasswordViewModel { Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        var savedEmail = HttpContext.Session.GetString("reset_email");
        var savedCode = HttpContext.Session.GetString("reset_code");

        if (string.IsNullOrWhiteSpace(savedEmail) || string.IsNullOrWhiteSpace(savedCode))
            return RedirectToAction(nameof(ForgotPassword));

        if (!ModelState.IsValid)
            return View(model);

        if (model.Code != savedCode)
        {
            ViewBag.Error = "Invalid reset code";
            model.Email = savedEmail;
            return View(model);
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == savedEmail && x.IsActive);
        if (user == null)
            return RedirectToAction(nameof(ForgotPassword));

        user.PasswordHash = AuthService.Sha256(model.NewPassword);
        await _db.SaveChangesAsync();

        HttpContext.Session.Remove("reset_email");
        HttpContext.Session.Remove("reset_code");

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync(HttpContext);
        return RedirectToAction(nameof(Login));
    }
}