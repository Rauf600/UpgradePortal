using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Filters;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.Services;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
[PermissionAuthorize("UserManagement")]
public class UserManagementController : Controller
{
    private readonly AppDbContext _db;

    public UserManagementController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("/UserManagement")]
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .Include(x => x.Role)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return View(users);
    }

    [HttpGet("/UserManagement/Create")]
    public async Task<IActionResult> Create()
    {
        await LoadRoles();
        ViewBag.Permissions = GetCorePermissions();

        return View(new UserCreateViewModel());
    }

    [HttpPost("/UserManagement/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (await _db.Users.AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email already exists.");
        }

        if (!ModelState.IsValid)
        {
            await LoadRoles();
            ViewBag.Permissions = GetCorePermissions();
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            RoleId = model.RoleId,
            PasswordHash = HashPassword(model.Password),
            TwoFactorEnabled = model.TwoFactorEnabled,
            IsActive = model.IsActive
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        foreach (var permission in model.SelectedPermissions)
        {
            _db.UserPermissions.Add(new UserPermission
            {
                UserId = user.UserId,
                PermissionCode = permission
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet("/UserManagement/Edit/{id}")]
    public async Task<IActionResult> Edit(long id)
    {
        var user = await _db.Users
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user == null)
            return NotFound();

        var model = new UserEditViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            TwoFactorEnabled = user.TwoFactorEnabled,
            IsActive = user.IsActive,
            SelectedPermissions = user.Permissions.Select(x => x.PermissionCode).ToList()
        };

        await LoadRoles();
        ViewBag.Permissions = GetCorePermissions();

        return View(model);
    }

    [HttpPost("/UserManagement/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRoles();
            ViewBag.Permissions = GetCorePermissions();
            return View(model);
        }

        var user = await _db.Users
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.UserId == model.UserId);

        if (user == null)
            return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.RoleId = model.RoleId;
        user.TwoFactorEnabled = model.TwoFactorEnabled;
        user.IsActive = model.IsActive;

        _db.UserPermissions.RemoveRange(user.Permissions);

        foreach (var permission in model.SelectedPermissions)
        {
            _db.UserPermissions.Add(new UserPermission
            {
                UserId = user.UserId,
                PermissionCode = permission
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet("/UserManagement/ResetPassword/{id}")]
    public async Task<IActionResult> ResetPassword(long id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        var model = new UserResetPasswordViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName
        };

        return View(model);
    }

    [HttpPost("/UserManagement/ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(UserResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FindAsync(model.UserId);

        if (user == null)
            return NotFound();

        user.PasswordHash = HashPassword(model.NewPassword);

        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost("/UserManagement/Activate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(long id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user != null)
        {
            user.IsActive = true;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    [HttpPost("/UserManagement/Deactivate/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(long id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user != null)
        {
            user.IsActive = false;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    private async Task LoadRoles()
    {
        var roles = await _db.Roles
            .OrderBy(x => x.RoleName)
            .ToListAsync();

        ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");
    }

    private static List<string> GetCorePermissions()
    {
        return new List<string>
        {
            "Dashboard",
            "Schedules",
            "ShellRequests",
            "Reports",
            "UserManagement",
            "Settings"
        };
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return AuthService.Sha256(password);

    }
    [HttpPost("/UserManagement/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user != null)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}