namespace UpgradePortal.Web.Models;

public class SendGridSettings
{
    public long SendGridSettingsId { get; set; }

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? Username { get; set; }

    public string? ApiKeyEncrypted { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }

    public bool Enabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}