using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpgradePortal.Web.Services;

namespace UpgradePortal.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ReportService _reportService;

    public DashboardController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("/Dashboard")]
    public async Task<IActionResult> Index()
    {
        var model = await _reportService.GetDashboardSummaryAsync();
        return View(model);
    }
}