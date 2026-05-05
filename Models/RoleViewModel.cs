using System.ComponentModel.DataAnnotations;

namespace UpgradePortal.Web.ViewModels;

public class RoleViewModel
{
    public long RoleId { get; set; }

    [Required]
    public string RoleName { get; set; } = "";

    public string? Description { get; set; }
}