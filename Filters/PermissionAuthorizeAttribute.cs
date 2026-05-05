using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;

namespace UpgradePortal.Web.Filters;

public class PermissionAuthorizeAttribute : TypeFilterAttribute
{
    public PermissionAuthorizeAttribute(string permission)
        : base(typeof(PermissionAuthorizeFilter))
    {
        Arguments = new object[] { permission };
    }
}

public class PermissionAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly AppDbContext _db;
    private readonly string _permission;

    public PermissionAuthorizeFilter(AppDbContext db, string permission)
    {
        _db = db;
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdText = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(userIdText, out var userId))
        {
            context.Result = new RedirectResult("/Auth/Login");
            return;
        }

        var user = await _db.Users
            .Include(x => x.Role)
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user == null || !user.IsActive)
        {
            context.Result = new RedirectResult("/Auth/Login");
            return;
        }

        var isAdmin = user.Role?.RoleName?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

        if (isAdmin)
            return;

        var hasPermission = user.Permissions.Any(x => x.PermissionCode == _permission);

        if (!hasPermission)
        {
            context.Result = new RedirectResult("/Dashboard");
        }
    }
}