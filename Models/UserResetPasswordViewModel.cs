using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class UserResetPasswordViewModel
{
    public long UserId { get; set; }

    public string FullName { get; set; } = "";

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = "";

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}