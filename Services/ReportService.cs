using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Models;

namespace UpgradePortal.Web.Services;

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryViewModel> GetDashboardSummaryAsync()
    {
        return new DashboardSummaryViewModel
        {
            TotalUsers = await _db.Users.CountAsync(x => x.IsActive),
            Total2FAUsers = await _db.Users.CountAsync(x => x.IsActive && x.TwoFactorEnabled),
            TotalSchedules = await _db.UpgradeSchedules.CountAsync(),
            TotalShellRequests = await _db.ShellRequests.CountAsync()
        };
    }
}