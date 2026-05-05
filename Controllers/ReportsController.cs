using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Filters;

namespace UpgradePortal.Web.Controllers;

[Authorize]
[PermissionAuthorize("Reports")]
public class ReportsController : Controller
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("/Reports")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalSchedules = await _db.UpgradeSchedules.CountAsync();
        ViewBag.PendingSchedules = await _db.UpgradeSchedules.CountAsync(x => x.Status == "pending");
        ViewBag.CompletedSchedules = await _db.UpgradeSchedules.CountAsync(x => x.Status == "completed");
        ViewBag.CancelledSchedules = await _db.UpgradeSchedules.CountAsync(x => x.Status == "cancelled");

        ViewBag.TotalShellRequests = await _db.ShellRequests.CountAsync();
        ViewBag.PendingShellRequests = await _db.ShellRequests.CountAsync(x => x.Status == "pending");
        ViewBag.CompletedShellRequests = await _db.ShellRequests.CountAsync(x => x.Status == "completed");
        ViewBag.CancelledShellRequests = await _db.ShellRequests.CountAsync(x => x.Status == "cancelled");

        return View("~/Views/ReportsView/Index.cshtml");
    }
}