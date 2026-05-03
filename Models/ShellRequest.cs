namespace UpgradePortal.Web.Models;

public class ShellRequest
{
    public long ShellRequestId { get; set; }
    public long CustomerId { get; set; }
    public long? CreatedByUserId { get; set; }

    public string ClinicName { get; set; } = "";
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? EmrId { get; set; }
    public string? TokenId { get; set; }
    public string? BaseContainer { get; set; }
    public string? ProfileVersion { get; set; }
    public int? NumProviders { get; set; }
    public int? NumIHServers { get; set; }
    public string? Region { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string? ClientRegistry { get; set; }
    public string? Integrations { get; set; }
    public string? Attachments { get; set; }
    public string Status { get; set; } = "";
    public string? Notes { get; set; }

    public Customer? Customer { get; set; }
    public User? CreatedByUser { get; set; }
}