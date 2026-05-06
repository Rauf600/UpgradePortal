using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpgradePortal.Web.Filters;

namespace UpgradePortal.Web.Controllers;

[Authorize]
[PermissionAuthorize("Settings")]
public class SettingsController : Controller
{
    [HttpGet("/Settings")]
    public IActionResult Index()
    {
        return View();
    }
}