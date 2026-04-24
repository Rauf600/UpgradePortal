namespace UpgradePortal.Web.Models;

public class UpgradeSchedule
{
    public long ScheduleId { get; set; }
    public long CustomerId { get; set; }
    public string HostingType { get; set; } = "";
    public string CurrentVersion { get; set; } = "";
    public string TargetVersion { get; set; } = "";
    public DateTime ScheduleDate { get; set; }
    public TimeSpan? ScheduleTime { get; set; }
    public string? TicketNumber { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "";

    public Customer? Customer { get; set; }
}