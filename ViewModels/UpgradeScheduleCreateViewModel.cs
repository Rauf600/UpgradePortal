using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class UpgradeScheduleCreateViewModel
{
    [Required]
    public string CustomerName { get; set; } = "";

    [Required]
    public string CustomerCode { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    public string? Region { get; set; }

    [Required]
    public string HostingType { get; set; } = "";

    [Required]
    public string CurrentVersion { get; set; } = "";

    [Required]
    public string TargetVersion { get; set; } = "";

    [Required]
    public DateTime ScheduleDate { get; set; }

    public TimeSpan? ScheduleTime { get; set; }

    public string? TicketNumber { get; set; }

    public string? Notes { get; set; }
}