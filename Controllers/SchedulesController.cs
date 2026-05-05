using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Filters;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
[PermissionAuthorize("Schedules")]
public class SchedulesController : Controller
{
    private readonly AppDbContext _db;

    public SchedulesController(AppDbContext db)
    {
        _db = db;
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
            return View(model);

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

        return RedirectToAction("Index");
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
        }

        return RedirectToAction("Index");
    }
}