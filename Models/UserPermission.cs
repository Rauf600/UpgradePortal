namespace UpgradePortal.Web.Models;

public class UserPermission
{
    public long UserPermissionId { get; set; }
    public long UserId { get; set; }
    public string PermissionCode { get; set; } = "";

    public User? User { get; set; }
}