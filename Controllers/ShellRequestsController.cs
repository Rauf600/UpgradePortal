using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
public class ShellRequestsController : Controller
{
    private readonly AppDbContext _db;

    public ShellRequestsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("/ShellRequests")]
    public async Task<IActionResult> Index(string sortField = "date", string sortOrder = "asc")
    {
        ViewBag.SortField = sortField;
        ViewBag.SortOrder = sortOrder;

        IQueryable<ShellRequest> query = _db.ShellRequests
            .Include(x => x.Customer);

        switch ((sortField ?? "").ToLower())
        {
            case "customer":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.Customer!.CustomerName)
                    : query.OrderBy(x => x.Customer!.CustomerName);
                break;

            case "clinic":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.ClinicName)
                    : query.OrderBy(x => x.ClinicName);
                break;

            case "version":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.ProfileVersion)
                    : query.OrderBy(x => x.ProfileVersion);
                break;

            case "registry":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.ClientRegistry)
                    : query.OrderBy(x => x.ClientRegistry);
                break;

            case "status":
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.Status)
                    : query.OrderBy(x => x.Status);
                break;

            case "date":
            default:
                query = sortOrder == "desc"
                    ? query.OrderByDescending(x => x.ExpectedDate)
                    : query.OrderBy(x => x.ExpectedDate);
                break;
        }

        var requests = await query.ToListAsync();
        return View(requests);
    }

    [HttpGet("/ShellRequests/Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("/ShellRequests/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShellRequestCreateViewModel model)
    {
        if (string.Equals(model.Region, "NB", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(model.ClientRegistry))
        {
            ModelState.AddModelError(nameof(model.ClientRegistry), "Client Registry is required for NB region.");
        }

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

        var integrations = new List<string>();
        if (model.IntegrationEFax) integrations.Add("eFax");
        if (model.IntegrationSMS) integrations.Add("SMS");
        if (model.IntegrationExcelleris) integrations.Add("Excelleris");
        if (model.IntegrationMCE) integrations.Add("MCE");
        if (model.IntegrationEHR) integrations.Add("EHR");
        if (model.IntegrationVSFormulary) integrations.Add("VS Formulary");
        if (model.IntegrationTelemedicine) integrations.Add("Telemedicine");
        if (model.IntegrationPrescribeIT) integrations.Add("PrescribeIT");
        if (model.IntegrationFDBFormulary) integrations.Add("FDB Formulary");
        if (model.IntegrationSendGridEmail) integrations.Add("SendGrid Email (2FA)");

        var attachmentNames = new List<string>();
        if (model.Attachments != null)
        {
            foreach (var file in model.Attachments)
            {
                if (file != null && !string.IsNullOrWhiteSpace(file.FileName))
                {
                    attachmentNames.Add(file.FileName);
                }
            }
        }

        var shellRequest = new ShellRequest
        {
            CustomerId = customer.CustomerId,
            ClinicName = model.ClinicName,
            Email = model.Email,
            Address = model.Address,
            Phone = model.Phone,
            EmrId = model.EmrId,
            TokenId = model.TokenId,
            BaseContainer = model.BaseContainer,
            ProfileVersion = model.ProfileVersion,
            NumProviders = model.NumProviders,
            NumIHServers = model.NumIHServers,
            Region = model.Region,
            ExpectedDate = model.ExpectedDate,
            ClientRegistry = model.ClientRegistry,
            Integrations = string.Join(", ", integrations),
            Attachments = string.Join(", ", attachmentNames),
            Notes = model.Notes,
            Status = "pending"
        };

        _db.ShellRequests.Add(shellRequest);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}