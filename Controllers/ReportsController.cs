using System.Text;
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
    public async Task<IActionResult> ExportCsv(
        string reportType,
        string? region,
        string? hostingType,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        var csv = new StringBuilder();

        string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        if (reportType == "Upgrade Schedules" || reportType == "Completed Upgrades")
        {
            var query = _db.UpgradeSchedules
                .Include(x => x.Customer)
                .Include(x => x.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(region) && region != "All Regions")
                query = query.Where(x => x.Customer != null && x.Customer.RegionCode == region);

            if (!string.IsNullOrWhiteSpace(hostingType) && hostingType != "All Types")
                query = query.Where(x => x.HostingType == hostingType);

            if (reportType == "Completed Upgrades")
                query = query.Where(x => x.Status == "completed");
            else if (!string.IsNullOrWhiteSpace(status) && status != "All Status")
                query = query.Where(x => x.Status == status);

            if (dateFrom.HasValue)
                query = query.Where(x => x.ScheduleDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(x => x.ScheduleDate <= dateTo.Value);

            var results = await query
                .OrderByDescending(x => x.ScheduleDate)
                .ToListAsync();

            csv.AppendLine("ScheduleId,CustomerName,CustomerCode,Region,Hosting,CurrentVersion,TargetVersion,Date,Time,Ticket,SubmittedBy,Status");

            foreach (var item in results)
            {
                csv.AppendLine(string.Join(",",
                    item.ScheduleId,
                    Escape(item.Customer?.CustomerName),
                    Escape(item.Customer?.CustomerCode),
                    Escape(item.Customer?.RegionCode),
                    Escape(item.HostingType),
                    Escape(item.CurrentVersion),
                    Escape(item.TargetVersion),
                    item.ScheduleDate.ToString("yyyy-MM-dd"),
                    item.ScheduleTime?.ToString(@"hh\:mm") ?? "",
                    Escape(item.TicketNumber),
                    Escape(item.CreatedByUser?.FullName),
                    Escape(item.Status)
                ));
            }
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

            var results = await query
                .OrderByDescending(x => x.ExpectedDate)
                .ToListAsync();

            csv.AppendLine("ShellRequestId,CustomerName,CustomerCode,ClinicName,ProfileVersion,ExpectedDate,ClientRegistry,SubmittedBy,Status");

            foreach (var item in results)
            {
                csv.AppendLine(string.Join(",",
                    item.ShellRequestId,
                    Escape(item.Customer?.CustomerName),
                    Escape(item.Customer?.CustomerCode),
                    Escape(item.ClinicName),
                    Escape(item.ProfileVersion),
                    item.ExpectedDate?.ToString("yyyy-MM-dd") ?? "",
                    Escape(item.ClientRegistry),
                    Escape(item.CreatedByUser?.FullName),
                    Escape(item.Status)
                ));
            }
        }
        else if (reportType == "User Activity")
        {
            var results = await _db.Users
                .Include(x => x.Role)
                .OrderBy(x => x.FullName)
                .ToListAsync();

            csv.AppendLine("UserId,FullName,Email,Role,2FA,Status");

            foreach (var item in results)
            {
                csv.AppendLine(string.Join(",",
                    item.UserId,
                    Escape(item.FullName),
                    Escape(item.Email),
                    Escape(item.Role?.RoleName),
                    item.TwoFactorEnabled ? "Enabled" : "Disabled",
                    item.IsActive ? "Active" : "Inactive"
                ));
            }
        }
        else
        {
            csv.AppendLine("No report selected.");
        }

        return File(
            Encoding.UTF8.GetBytes(csv.ToString()),
            "text/csv",
            $"report_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
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