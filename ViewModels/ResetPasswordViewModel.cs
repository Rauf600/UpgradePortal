using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class ResetPasswordViewModel
{
    public string Email { get; set; } = "";

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = "";

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = "";

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = "";
}