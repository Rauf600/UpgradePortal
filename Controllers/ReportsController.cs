using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
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
        await LoadChartData();
        return View("~/Views/ReportsView/Index.cshtml");
    }

    [HttpPost("/Reports")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        string reportType,
        string? region,
        string? hostingType,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        ViewBag.ReportType = reportType;
        ViewBag.Region = region;
        ViewBag.HostingType = hostingType;
        ViewBag.Status = status;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

        if (reportType == "Upgrade Schedules" || reportType == "Completed Upgrades")
        {
            var query = _db.UpgradeSchedules
                .Include(x => x.Customer)
                .Include(x => x.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region) && region != "All Regions")
                query = query.Where(x => x.Customer != null && x.Customer.RegionCode == region);

            if (!string.IsNullOrWhiteSpace(hostingType) && hostingType != "All Types")
                query = query.Where(x => x.HostingType != null && x.HostingType == hostingType);

            if (reportType == "Completed Upgrades")
                query = query.Where(x => x.Status == "completed");
            else if (!string.IsNullOrWhiteSpace(status) && status != "All Status")
                query = query.Where(x => x.Status == status);

            if (dateFrom.HasValue)
                query = query.Where(x => x.ScheduleDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(x => x.ScheduleDate <= dateTo.Value);

            ViewBag.ScheduleResults = await query
                .OrderByDescending(x => x.ScheduleDate)
                .ToListAsync();
        }
        else if (reportType == "Shell Requests")
        {
            var query = _db.ShellRequests
                .Include(x => x.Customer)
                .Include(x => x.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region) && region != "All Regions")
                query = query.Where(x => x.Region == region);

            if (!string.IsNullOrWhiteSpace(status) && status != "All Status")
                query = query.Where(x => x.Status == status);

            if (dateFrom.HasValue)
                query = query.Where(x => x.ExpectedDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(x => x.ExpectedDate <= dateTo.Value);

            ViewBag.ShellResults = await query
                .OrderByDescending(x => x.ExpectedDate)
                .ToListAsync();
        }
        else if (reportType == "User Activity")
        {
            ViewBag.UserResults = await _db.Users
                .Include(x => x.Role)
                .OrderBy(x => x.FullName)
                .ToListAsync();
        }

        await LoadChartData();

        return View("~/Views/ReportsView/Index.cshtml");
    }

    [HttpPost("/Reports/ExportCsv")]
    [ValidateAntiForgeryToken]
    public IActionResult ExportCsv()
    {
        var csv = new StringBuilder();
        csv.AppendLine("Report export will be enhanced in future version.");

        return File(
            Encoding.UTF8.GetBytes(csv.ToString()),
            "text/csv",
            "report.csv"
        );
    }

    private async Task LoadChartData()
    {
        ViewBag.SchedulePending = await _db.UpgradeSchedules.CountAsync(x => x.Status == "pending");
        ViewBag.ScheduleCompleted = await _db.UpgradeSchedules.CountAsync(x => x.Status == "completed");
        ViewBag.ScheduleCancelled = await _db.UpgradeSchedules.CountAsync(x => x.Status == "cancelled");

        ViewBag.ShellPending = await _db.ShellRequests.CountAsync(x => x.Status == "pending");
        ViewBag.ShellCompleted = await _db.ShellRequests.CountAsync(x => x.Status == "completed");
        ViewBag.ShellCancelled = await _db.ShellRequests.CountAsync(x => x.Status == "cancelled");
    }
}