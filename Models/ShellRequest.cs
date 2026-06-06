using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.Models;

public class ShellRequest
{
    [Key]
    public long ShellRequestId { get; set; }

    public long CustomerId { get; set; }

    public long? CreatedByUserId { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? EmrId { get; set; }

    public string? TokenId { get; set; }

    public string? BaseContainer { get; set; }

    public string? ProfileVersion { get; set; }

    public int NumUsers { get; set; }

    public int NumIHServers { get; set; }

    public int NumDedicatedServers { get; set; }

    public int TotalServers { get; set; }

    public string? Region { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string? ClientRegistry { get; set; }

    public string? Integrations { get; set; }

    public string? Attachments { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = "pending";

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Customer? Customer { get; set; }

    public User? CreatedByUser { get; set; }
}