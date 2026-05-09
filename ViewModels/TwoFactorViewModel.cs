using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class TwoFactorViewModel
{
    public string Email { get; set; } = "";

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = "";
}