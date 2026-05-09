namespace UpgradePortal.Web.Models;

public class AppSettings
{
    public long AppSettingsId { get; set; }

    public bool JiraEnabled { get; set; }
    public string? JiraBaseUrl { get; set; }
    public string? JiraProjectKey { get; set; }
    public string? JiraUsername { get; set; }
    public string? JiraApiToken { get; set; }

    public DateTime UpdatedAt { get; set; }
}