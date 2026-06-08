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
[PermissionAuthorize("ShellRequests")]
public class ShellRequestsController : Controller
{
    private readonly AppDbContext _db;
    private readonly SendGridEmailService _emailService;
    private readonly ILogger<ShellRequestsController> _logger;

    public ShellRequestsController(
        AppDbContext db,
        SendGridEmailService emailService,
        ILogger<ShellRequestsController> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("/ShellRequests")]
    public async Task<IActionResult> Index(string sortField = "date", string sortOrder = "desc")
    {
        ViewBag.SortField = sortField;
        ViewBag.SortOrder = sortOrder;

        IQueryable<ShellRequest> query = _db.ShellRequests
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser);

        query = sortField.ToLower() switch
        {
            "id" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.ShellRequestId)
                : query.OrderBy(x => x.ShellRequestId),

            "customer" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Customer!.CustomerName)
                : query.OrderBy(x => x.Customer!.CustomerName),

            "customerid" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Customer!.CustomerCode)
                : query.OrderBy(x => x.Customer!.CustomerCode),

            "clinic" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.ClinicName)
                : query.OrderBy(x => x.ClinicName),

            "profileversion" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.ProfileVersion)
                : query.OrderBy(x => x.ProfileVersion),

            "clientregistry" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.ClientRegistry)
                : query.OrderBy(x => x.ClientRegistry),

            "submittedby" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.CreatedByUser!.FullName)
                : query.OrderBy(x => x.CreatedByUser!.FullName),

            "status" => sortOrder == "desc"
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            _ => sortOrder == "desc"
                ? query.OrderByDescending(x => x.ExpectedDate)
                : query.OrderBy(x => x.ExpectedDate)
        };

        return View(await query.ToListAsync());
    }

    [HttpGet("/ShellRequests/Create")]
    public IActionResult Create()
    {
        return View(new ShellRequestCreateViewModel
        {
            NumUsers = 1,
            NumIHServers = 1,
            NumDedicatedServers = 1,
            TotalServers = 2
        });
    }

    [HttpPost("/ShellRequests/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShellRequestCreateViewModel model)
    {
        bool isNbRegion = string.Equals(model.Region, "NB", StringComparison.OrdinalIgnoreCase);
        bool isRestrictedRegion =
            string.Equals(model.Region, "BC", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(model.Region, "AU", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(model.Region, "NZ", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(model.Region, "ON", StringComparison.OrdinalIgnoreCase);

        if (isNbRegion)
        {
            if (string.IsNullOrWhiteSpace(model.ClientRegistry))
            {
                ModelState.AddModelError(
                    nameof(model.ClientRegistry),
                    "Client Registry is required for NB region.");
            }
        }
        else
        {
            model.ClientRegistry = null;
            ModelState.Remove(nameof(model.ClientRegistry));
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Invalid shell request submission from {User}.",
                User.Identity?.Name ?? "Unknown");

            return View(model);
        }

        try
        {
            if (isRestrictedRegion)
            {
                model.EmrId = null;
                model.TokenId = null;
                model.ClientRegistry = null;
                model.IntegrationEHR = false;
                model.IntegrationMCE = false;
            }

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

            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long? createdByUserId = long.TryParse(userIdValue, out var parsedUserId)
                ? parsedUserId
                : null;

            int ihServers = CalculateIHServers(model.NumUsers);
            int dedicatedServers = 1;
            int totalServers = ihServers + dedicatedServers;

            var now = DateTime.Now;

            var shellRequest = new ShellRequest
            {
                CustomerId = customer.CustomerId,
                CreatedByUserId = createdByUserId,
                ClinicName = model.ClinicName,
                Email = model.Email,
                Address = model.Address,
                Phone = model.Phone,
                EmrId = model.EmrId,
                TokenId = model.TokenId,
                BaseContainer = model.BaseContainer,
                ProfileVersion = model.ProfileVersion,
                NumUsers = model.NumUsers,
                NumIHServers = ihServers,
                NumDedicatedServers = dedicatedServers,
                TotalServers = totalServers,
                Region = model.Region,
                ExpectedDate = model.ExpectedDate,
                ClientRegistry = model.ClientRegistry,
                Integrations = string.Join(", ", integrations),
                Attachments = string.Join(", ", attachmentNames),
                Notes = model.Notes,
                Status = "pending",
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.ShellRequests.Add(shellRequest);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Shell request submitted by {User} for {ClinicName}.",
                User.Identity?.Name ?? "Unknown",
                model.ClinicName);

            await _emailService.SendTechOpsNotificationAsync(
                "rauf.ibrahimkhail@intrahealth.com",
                "Shell Request",
                User.Identity?.Name ?? "Unknown User",
                $"New shell request submitted for {model.ClinicName}"
            );

            _logger.LogInformation(
                "TechOps notification sent for shell request {ClinicName}.",
                model.ClinicName);

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating shell request for {ClinicName}.",
                model.ClinicName);

            throw;
        }
    }

    [HttpGet("/ShellRequests/Details/{id}")]
    public async Task<IActionResult> Details(long id)
    {
        var request = await _db.ShellRequests
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.ShellRequestId == id);

        if (request == null)
        {
            _logger.LogWarning("Shell request {RequestId} not found.", id);
            return NotFound();
        }

        return View(request);
    }

    [HttpPost("/ShellRequests/Complete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(long id)
    {
        var request = await _db.ShellRequests.FindAsync(id);

        if (request != null)
        {
            request.Status = "completed";
            request.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Shell request {RequestId} marked as completed by {User}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }

        return RedirectToAction("Index");
    }

    [HttpPost("/ShellRequests/Cancel/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        var request = await _db.ShellRequests.FindAsync(id);

        if (request != null)
        {
            request.Status = "cancelled";
            request.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Shell request {RequestId} marked as cancelled by {User}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }

        return RedirectToAction("Index");
    }

    private static int CalculateIHServers(int userCount)
    {
        if (userCount <= 0)
            return 1;

        return (int)Math.Ceiling(userCount / 5.0);
    }
}