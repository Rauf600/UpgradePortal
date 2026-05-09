namespace UpgradePortal.Web.ViewModels;

public class SettingsViewModel
{
    public bool JiraEnabled { get; set; }
    public string? JiraBaseUrl { get; set; }
    public string? JiraProjectKey { get; set; }
    public string? JiraUsername { get; set; }
    public string? JiraApiToken { get; set; }

    public bool SendGridEnabled { get; set; }
    public string? SendGridApiKey { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? TestEmail { get; set; }
}