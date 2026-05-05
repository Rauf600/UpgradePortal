using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class UserEditViewModel
{
    public long UserId { get; set; }

    [Required]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    public string? PhoneNumber { get; set; }

    [Required]
    public long RoleId { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool IsActive { get; set; }
    public List<string> SelectedPermissions { get; set; } = new();
}