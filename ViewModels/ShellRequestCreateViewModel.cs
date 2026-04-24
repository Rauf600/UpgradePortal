using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpgradePortal.Web.ViewModels;

public class ShellRequestCreateViewModel
{
    [Required]
    public string CustomerCode { get; set; } = "";

    [Required]
    public string CustomerName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string ClinicName { get; set; } = "";

    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? EmrId { get; set; }
    public string? TokenId { get; set; }
    public string? BaseContainer { get; set; }
    public string? Region { get; set; }
    public string? ProfileVersion { get; set; }

    public int? NumProviders { get; set; }
    public int? NumIHServers { get; set; }

    [Required]
    public DateTime? ExpectedDate { get; set; }

    public string? ClientRegistry { get; set; }

    public string? Notes { get; set; }

    public bool IntegrationEFax { get; set; }
    public bool IntegrationSMS { get; set; }
    public bool IntegrationExcelleris { get; set; }
    public bool IntegrationMCE { get; set; }
    public bool IntegrationEHR { get; set; }
    public bool IntegrationVSFormulary { get; set; }
    public bool IntegrationTelemedicine { get; set; }
    public bool IntegrationPrescribeIT { get; set; }
    public bool IntegrationFDBFormulary { get; set; }
    public bool IntegrationSendGridEmail { get; set; }

    public List<IFormFile>? Attachments { get; set; }
}