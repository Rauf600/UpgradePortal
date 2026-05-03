using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class UserCreateViewModel
{
    [Required]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required]
    public long RoleId { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = "";

    public bool TwoFactorEnabled { get; set; }

    public bool IsActive { get; set; } = true;

}