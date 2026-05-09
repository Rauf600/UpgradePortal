using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using UpgradePortal.Web.Data;

namespace UpgradePortal.Web.Services;

public class SendGridEmailService
{
    private readonly AppDbContext _db;

    public SendGridEmailService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> SendTwoFactorCodeAsync(string toEmail, string code)
    {
        var settings = await _db.SendGridSettings
            .FirstOrDefaultAsync(x => x.Enabled);

        if (settings == null)
            return false;

        if (string.IsNullOrWhiteSpace(settings.ApiKeyEncrypted) ||
            string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            return false;
        }

        var client = new SendGridClient(settings.ApiKeyEncrypted);

        var from = new EmailAddress(
            settings.FromEmail,
            string.IsNullOrWhiteSpace(settings.FromName) ? "Upgrade Portal" : settings.FromName);

        var to = new EmailAddress(toEmail);

        var subject = "Your Upgrade Portal 2FA Code";
        var plainTextContent = $"Your verification code is: {code}";
        var htmlContent = $@"
            <div style='font-family:Arial,sans-serif;'>
                <p>Your verification code is:</p>
                <h2 style='letter-spacing:2px;'>{code}</h2>
                <p>This code will expire soon.</p>
            </div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendPasswordResetCodeAsync(string toEmail, string code)
    {
        var settings = await _db.SendGridSettings
            .FirstOrDefaultAsync(x => x.Enabled);

        if (settings == null)
            return false;

        if (string.IsNullOrWhiteSpace(settings.ApiKeyEncrypted) ||
            string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            return false;
        }

        var client = new SendGridClient(settings.ApiKeyEncrypted);

        var from = new EmailAddress(
            settings.FromEmail,
            string.IsNullOrWhiteSpace(settings.FromName) ? "Upgrade Portal" : settings.FromName);

        var to = new EmailAddress(toEmail);

        var subject = "Your Upgrade Portal Password Reset Code";
        var plainTextContent = $"Your password reset code is: {code}";
        var htmlContent = $@"
            <div style='font-family:Arial,sans-serif;'>
                <p>Your password reset code is:</p>
                <h2 style='letter-spacing:2px;'>{code}</h2>
                <p>This code will expire soon.</p>
            </div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
    }
}