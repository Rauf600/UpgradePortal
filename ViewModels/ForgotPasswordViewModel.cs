using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
}