using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UpgradePortal.Web.ViewModels;

public class ShellRequestCreateViewModel
{
    [Required]
    public string CustomerCode { get; set; } = string.Empty;

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string ClinicName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? EmrId { get; set; }

    public string? TokenId { get; set; }

    public string? BaseContainer { get; set; }

    public string? ProfileVersion { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Number of Users must be at least 1.")]
    public int NumUsers { get; set; }

    public int NumIHServers { get; set; }

    public int NumDedicatedServers { get; set; }

    public int TotalServers { get; set; }

    [Required]
    public string Region { get; set; } = string.Empty;

    public DateTime? ExpectedDate { get; set; }

    public string? ClientRegistry { get; set; }

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

    public string? Notes { get; set; }
}