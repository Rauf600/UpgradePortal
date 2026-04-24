namespace UpgradePortal.Web.Models;

public class User
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorEmail { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}