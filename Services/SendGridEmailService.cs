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

    public async Task<bool> SendTechOpsNotificationAsync(
        string techOpsEmail,
        string requestType,
        string submittedBy,
        string summary)
    {
        var settings = await _db.SendGridSettings
            .FirstOrDefaultAsync(x => x.Enabled);

        if (settings == null)
            return false;

        if (string.IsNullOrWhiteSpace(settings.ApiKeyEncrypted) ||
            string.IsNullOrWhiteSpace(settings.FromEmail) ||
            string.IsNullOrWhiteSpace(techOpsEmail))
        {
            return false;
        }

        var client = new SendGridClient(settings.ApiKeyEncrypted);

        var from = new EmailAddress(
            settings.FromEmail,
            string.IsNullOrWhiteSpace(settings.FromName) ? "Upgrade Portal" : settings.FromName);

        var to = new EmailAddress(techOpsEmail);

        var subject = $"New {requestType} Submitted in Upgrade Portal";
        var plainTextContent =
            $"A new {requestType} has been submitted.\n\nSubmitted By: {submittedBy}\nSummary: {summary}";

        var htmlContent = $@"
            <div style='font-family:Arial,sans-serif;'>
                <h2>New {requestType} Submitted</h2>
                <p><strong>Submitted By:</strong> {submittedBy}</p>
                <p><strong>Summary:</strong> {summary}</p>
                <p>Please log in to the portal to review the request.</p>
            </div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendShellRequestStatusUpdateAsync(
        string toEmail,
        string requesterName,
        string clinicName,
        string status,
        string updatedBy)
    {
        var settings = await _db.SendGridSettings
            .FirstOrDefaultAsync(x => x.Enabled);

        if (settings == null)
            return false;

        if (string.IsNullOrWhiteSpace(settings.ApiKeyEncrypted) ||
            string.IsNullOrWhiteSpace(settings.FromEmail) ||
            string.IsNullOrWhiteSpace(toEmail))
        {
            return false;
        }

        var client = new SendGridClient(settings.ApiKeyEncrypted);

        var from = new EmailAddress(
            settings.FromEmail,
            string.IsNullOrWhiteSpace(settings.FromName) ? "Upgrade Portal" : settings.FromName);

        var to = new EmailAddress(toEmail);

        var statusLabel = status == "completed" ? "Completed" : "Cancelled";
        var statusColor = status == "completed" ? "#16a34a" : "#dc2626";
        var statusIcon = status == "completed" ? "✅" : "❌";

        var subject = $"Shell Request {statusLabel} - {clinicName}";

        var plainTextContent =
            $"Dear {requesterName},\n\n" +
            $"Your shell request for {clinicName} has been {statusLabel.ToLower()}.\n\n" +
            $"Updated By: {updatedBy}\n\n" +
            $"Please log in to the portal for more details.";

        var htmlContent = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
                <div style='background:#1f2a44;padding:24px;border-radius:8px 8px 0 0;'>
                    <h2 style='color:white;margin:0;'>Upgrade Portal</h2>
                </div>
                <div style='background:#ffffff;padding:32px;border-radius:0 0 8px 8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
                    <h3 style='color:#0f172a;margin-top:0;'>{statusIcon} Shell Request {statusLabel}</h3>
                    <p style='color:#475569;'>Dear <strong>{requesterName}</strong>,</p>
                    <p style='color:#475569;'>Your shell request for the following clinic has been <strong style='color:{statusColor};'>{statusLabel.ToLower()}</strong>:</p>
                    <div style='background:#f8fafc;border-left:4px solid {statusColor};padding:16px;border-radius:4px;margin:20px 0;'>
                        <p style='margin:0;font-size:18px;font-weight:bold;color:#0f172a;'>{clinicName}</p>
                    </div>
                    <p style='color:#475569;'><strong>Updated By:</strong> {updatedBy}</p>
                    <p style='color:#475569;'>Please log in to the portal to view the full details of your request.</p>
                    <div style='margin-top:24px;padding-top:24px;border-top:1px solid #e5e7eb;'>
                        <p style='color:#94a3b8;font-size:12px;margin:0;'>This is an automated message from Upgrade Portal. Please do not reply to this email.</p>
                    </div>
                </div>
            </div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendScheduleStatusUpdateAsync(
        string toEmail,
        string requesterName,
        string customerName,
        string scheduleDate,
        string currentVersion,
        string targetVersion,
        string status,
        string updatedBy)
    {
        var settings = await _db.SendGridSettings
            .FirstOrDefaultAsync(x => x.Enabled);

        if (settings == null)
            return false;

        if (string.IsNullOrWhiteSpace(settings.ApiKeyEncrypted) ||
            string.IsNullOrWhiteSpace(settings.FromEmail) ||
            string.IsNullOrWhiteSpace(toEmail))
        {
            return false;
        }

        var client = new SendGridClient(settings.ApiKeyEncrypted);

        var from = new EmailAddress(
            settings.FromEmail,
            string.IsNullOrWhiteSpace(settings.FromName) ? "Upgrade Portal" : settings.FromName);

        var to = new EmailAddress(toEmail);

        var statusLabel = status == "completed" ? "Completed" : "Cancelled";
        var statusColor = status == "completed" ? "#16a34a" : "#dc2626";
        var statusIcon = status == "completed" ? "✅" : "❌";

        var subject = $"Upgrade Schedule {statusLabel} - {customerName}";

        var plainTextContent =
            $"Dear {requesterName},\n\n" +
            $"The upgrade schedule for {customerName} has been {statusLabel.ToLower()}.\n\n" +
            $"Schedule Date: {scheduleDate}\n" +
            $"Current Version: {currentVersion}\n" +
            $"Target Version: {targetVersion}\n" +
            $"Updated By: {updatedBy}\n\n" +
            $"Please log in to the portal for more details.";

        var htmlContent = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
                <div style='background:#1f2a44;padding:24px;border-radius:8px 8px 0 0;'>
                    <h2 style='color:white;margin:0;'>Upgrade Portal</h2>
                </div>
                <div style='background:#ffffff;padding:32px;border-radius:0 0 8px 8px;box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
                    <h3 style='color:#0f172a;margin-top:0;'>{statusIcon} Upgrade Schedule {statusLabel}</h3>
                    <p style='color:#475569;'>Dear <strong>{requesterName}</strong>,</p>
                    <p style='color:#475569;'>The upgrade schedule for <strong>{customerName}</strong> has been <strong style='color:{statusColor};'>{statusLabel.ToLower()}</strong>.</p>
                    <div style='background:#f8fafc;border-left:4px solid {statusColor};padding:16px;border-radius:4px;margin:20px 0;'>
                        <table style='width:100%;border-collapse:collapse;'>
                            <tr>
                                <td style='padding:6px 0;color:#64748b;font-size:13px;'>Customer</td>
                                <td style='padding:6px 0;font-weight:bold;color:#0f172a;'>{customerName}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#64748b;font-size:13px;'>Schedule Date</td>
                                <td style='padding:6px 0;font-weight:bold;color:#0f172a;'>{scheduleDate}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#64748b;font-size:13px;'>Current Version</td>
                                <td style='padding:6px 0;font-weight:bold;color:#0f172a;'>{currentVersion}</td>
                            </tr>
                            <tr>
                                <td style='padding:6px 0;color:#64748b;font-size:13px;'>Target Version</td>
                                <td style='padding:6px 0;font-weight:bold;color:#0f172a;'>{targetVersion}</td>
                            </tr>
                        </table>
                    </div>
                    <p style='color:#475569;'><strong>Updated By:</strong> {updatedBy}</p>
                    <p style='color:#475569;'>Please log in to the portal to view the full details of your schedule.</p>
                    <div style='margin-top:24px;padding-top:24px;border-top:1px solid #e5e7eb;'>
                        <p style='color:#94a3b8;font-size:12px;margin:0;'>This is an automated message from Upgrade Portal. Please do not reply to this email.</p>
                    </div>
                </div>
            </div>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
    }
}