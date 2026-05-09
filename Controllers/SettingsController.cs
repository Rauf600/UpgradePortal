using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Filters;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.Services;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
[PermissionAuthorize("Settings")]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;
    private readonly EmailService _emailService;

    public SettingsController(AppDbContext db, EmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    [HttpGet("/Settings")]
    public async Task<IActionResult> Index()
    {
        var appSettings = await _db.AppSettings.FirstOrDefaultAsync();
        var sendGrid = await _db.SendGridSettings.FirstOrDefaultAsync();

        var model = new SettingsViewModel
        {
            JiraEnabled = appSettings?.JiraEnabled ?? false,
            JiraBaseUrl = appSettings?.JiraBaseUrl,
            JiraProjectKey = appSettings?.JiraProjectKey,
            JiraUsername = appSettings?.JiraUsername,
            JiraApiToken = appSettings?.JiraApiToken,

            SendGridEnabled = sendGrid?.Enabled ?? false,
            SendGridApiKey = sendGrid?.ApiKeyEncrypted,
            FromEmail = sendGrid?.FromEmail,
            FromName = sendGrid?.FromName
        };

        return View(model);
    }

    [HttpPost("/Settings/SaveJira")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveJira(SettingsViewModel model)
    {
        var appSettings = await _db.AppSettings.FirstOrDefaultAsync();

        if (appSettings == null)
        {
            appSettings = new AppSettings();
            _db.AppSettings.Add(appSettings);
        }

        appSettings.JiraEnabled = model.JiraEnabled;
        appSettings.JiraBaseUrl = model.JiraBaseUrl;
        appSettings.JiraProjectKey = model.JiraProjectKey;
        appSettings.JiraUsername = model.JiraUsername;
        appSettings.JiraApiToken = model.JiraApiToken;
        appSettings.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Jira settings saved successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost("/Settings/SaveSendGrid")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSendGrid(SettingsViewModel model)
    {
        var sendGrid = await _db.SendGridSettings.FirstOrDefaultAsync();

        if (sendGrid == null)
        {
            sendGrid = new SendGridSettings();
            _db.SendGridSettings.Add(sendGrid);
        }

        sendGrid.Enabled = model.SendGridEnabled;
        sendGrid.ApiKeyEncrypted = model.SendGridApiKey;
        sendGrid.FromEmail = model.FromEmail;
        sendGrid.FromName = model.FromName;
        sendGrid.Username = "apikey";
        sendGrid.SmtpHost = "smtp.sendgrid.net";
        sendGrid.SmtpPort = 587;
        sendGrid.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = "SendGrid settings saved successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost("/Settings/SendTestEmail")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendTestEmail(SettingsViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.TestEmail))
        {
            TempData["Error"] = "Please enter a test email address.";
            return RedirectToAction("Index");
        }

        var tempSettings = new SendGridSettings
        {
            Enabled = model.SendGridEnabled,
            ApiKeyEncrypted = model.SendGridApiKey,
            FromEmail = model.FromEmail,
            FromName = model.FromName,
            Username = "apikey",
            SmtpHost = "smtp.sendgrid.net",
            SmtpPort = 587
        };

        var error = await _emailService.SendTestEmailAsync(tempSettings, model.TestEmail);

        if (!string.IsNullOrWhiteSpace(error))
        {
            TempData["Error"] = error;
        }
        else
        {
            TempData["Success"] = $"Test email sent successfully to {model.TestEmail}.";
        }

        return RedirectToAction("Index");
    }
}