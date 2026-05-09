using System.Net;
using System.Net.Mail;
using UpgradePortal.Web.Models;

namespace UpgradePortal.Web.Services;

public class EmailService
{
    public async Task<string?> SendTestEmailAsync(SendGridSettings settings, string toEmail)
    {
        if (settings == null)
            return "SendGrid settings are missing.";

        if (!settings.Enabled)
            return "Enable SendGrid first.";

        if (string.IsNullOrWhiteSpace(toEmail))
            return "Please enter a test email address.";

        if (string.IsNullOrWhiteSpace(settings.ApiKeyEncrypted))
            return "SendGrid API Key is required.";

        if (string.IsNullOrWhiteSpace(settings.FromEmail))
            return "From Email is required.";

        try
        {
            using var client = new SmtpClient("smtp.sendgrid.net", 587)
            {
                Credentials = new NetworkCredential("apikey", settings.ApiKeyEncrypted),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(settings.FromEmail, settings.FromName ?? "Upgrade Portal"),
                Subject = "Upgrade Portal Test Email",
                Body = "<p>This is a test email from Upgrade Portal.</p><p>Your SendGrid configuration is working.</p>",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
            return null;
        }
        catch (Exception ex)
        {
            return $"Unable to send test email: {ex.Message}";
        }
    }
}