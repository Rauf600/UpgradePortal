using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Filters;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.Services;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
[PermissionAuthorize("Schedules")]
public class SchedulesController : Controller
{
    private readonly AppDbContext _db;
    private readonly SendGridEmailService _emailService;
    private readonly ILogger<SchedulesController> _logger;

    public SchedulesController(
        AppDbContext db,
        SendGridEmailService emailService,
        ILogger<SchedulesController> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("/Schedules")]
    public async Task<IActionResult> Index(string sortField = "date", string sortOrder = "asc")
    {
        ViewBag.SortField = sortField;
        ViewBag.SortOrder = sortOrder;

        IQueryable<UpgradeSchedule> query = _db.UpgradeSchedules
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser);

        query = sortField.ToLower() switch
        {
            "customer" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Customer!.CustomerName)
                : query.OrderBy(x => x.Customer!.CustomerName),

            "hosting" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.HostingType)
                : query.OrderBy(x => x.HostingType),

            "status" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            _ => sortOrder == "desc"
                ? query.OrderByDescending(x => x.ScheduleDate).ThenByDescending(x => x.ScheduleTime)
                : query.OrderBy(x => x.ScheduleDate).ThenBy(x => x.ScheduleTime)
        };

        return View(await query.ToListAsync());
    }

    [HttpGet("/Schedules/Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("/Schedules/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UpgradeScheduleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Invalid schedule submission from {User}.",
                User.Identity?.Name ?? "Unknown");

            return View(model);
        }

        try
        {
            var customer = await _db.Customers
                .FirstOrDefaultAsync(x => x.CustomerCode == model.CustomerCode);

            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerCode = model.CustomerCode,
                    CustomerName = model.CustomerName,
                    PrimaryEmail = model.Email,
                    RegionCode = model.Region
                };

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Created new customer {CustomerCode}.",
                    model.CustomerCode);
            }

            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long? createdByUserId = long.TryParse(userIdValue, out var parsedUserId)
                ? parsedUserId
                : null;

            var schedule = new UpgradeSchedule
            {
                CustomerId = customer.CustomerId,
                CreatedByUserId = createdByUserId,
                HostingType = model.HostingType,
                CurrentVersion = model.CurrentVersion,
                TargetVersion = model.TargetVersion,
                ScheduleDate = model.ScheduleDate,
                ScheduleTime = model.ScheduleTime,
                TicketNumber = model.TicketNumber,
                Notes = model.Notes,
                Status = "pending"
            };

            _db.UpgradeSchedules.Add(schedule);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "New schedule created for {CustomerName} by {User}.",
                customer.CustomerName,
                User.Identity?.Name ?? "Unknown");

            await _emailService.SendTechOpsNotificationAsync(
                "rauf.ibrahimkhail@intrahealth.com",
                "Upgrade Schedule",
                User.Identity?.Name ?? "Unknown User",
                $"New upgrade schedule submitted for {customer.CustomerName} on {schedule.ScheduleDate:yyyy-MM-dd}"
            );

            _logger.LogInformation(
                "TechOps notification sent for schedule {ScheduleId}.",
                schedule.ScheduleId);

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating schedule for {CustomerCode}.",
                model.CustomerCode);

            throw;
        }
    }

    [HttpGet("/Schedules/Details/{id}")]
    public async Task<IActionResult> Details(long id)
    {
        var schedule = await _db.UpgradeSchedules
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.ScheduleId == id);

        if (schedule == null)
            return NotFound();

        return View(schedule);
    }

    [HttpPost("/Schedules/Complete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(long id)
    {
        var schedule = await _db.UpgradeSchedules.FindAsync(id);

        if (schedule != null)
        {
            schedule.Status = "completed";
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Schedule {ScheduleId} marked as completed by {User}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }

        return RedirectToAction("Index");
    }

    [HttpPost("/Schedules/Cancel/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var schedule = await _db.UpgradeSchedules.FindAsync(id);

        if (schedule != null)
        {
            schedule.Status = "cancelled";
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Schedule {ScheduleId} marked as cancelled by {User}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }

        return RedirectToAction("Index");
    }
}