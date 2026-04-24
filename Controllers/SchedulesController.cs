using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
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
            .Include(x => x.Customer);

        switch ((sortField ?? "").ToLower())
        {
            case "customer":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.Customer!.CustomerName)
                    : query.OrderBy(x => x.Customer!.CustomerName);
                break;

            case "hosting":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.HostingType)
                    : query.OrderBy(x => x.HostingType);
                break;

            case "currentversion":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.CurrentVersion)
                    : query.OrderBy(x => x.CurrentVersion);
                break;

            case "targetversion":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.TargetVersion)
                    : query.OrderBy(x => x.TargetVersion);
                break;

            case "time":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.ScheduleTime)
                    : query.OrderBy(x => x.ScheduleTime);
                break;

            case "ticket":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.TicketNumber)
                    : query.OrderBy(x => x.TicketNumber);
                break;

            case "status":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.Status)
                    : query.OrderBy(x => x.Status);
                break;

            case "date":
            default:
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.ScheduleDate).ThenByDescending(x => x.ScheduleTime)
                    : query.OrderBy(x => x.ScheduleDate).ThenBy(x => x.ScheduleTime);
                break;
        }

        var schedules = await query.ToListAsync();
        return View(schedules);
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

        var schedule = new UpgradeSchedule
        {
            CustomerId = customer.CustomerId,
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
}