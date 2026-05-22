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
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(AppDbContext db, ILogger<UserManagementController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("/UserManagement")]
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .Include(x => x.Role)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        _logger.LogInformation(
            "User management list opened by {User}.",
            User.Identity?.Name ?? "Unknown");

        return View(users);
    }

    [HttpGet("/UserManagement/Create")]
    public async Task<IActionResult> Create()
    {
        await LoadRoles();
        ViewBag.Permissions = GetCorePermissions();

        _logger.LogInformation(
            "User create page opened by {User}.",
            User.Identity?.Name ?? "Unknown");

        return View(new UserCreateViewModel());
    }

    [HttpPost("/UserManagement/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (await _db.Users.AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email already exists.");
            _logger.LogWarning(
                "Attempt to create duplicate user with email {Email}.",
                model.Email);
        }

        if (!ModelState.IsValid)
        {
            await LoadRoles();
            ViewBag.Permissions = GetCorePermissions();

            _logger.LogWarning(
                "Invalid user creation submitted by {User}.",
                User.Identity?.Name ?? "Unknown");

            return View(model);
        }

        try
        {
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

            _logger.LogInformation(
                "New user created: {FullName} ({Email}) by {Admin}.",
                user.FullName,
                user.Email,
                User.Identity?.Name ?? "Unknown");

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating user {Email}.",
                model.Email);

            throw;
        }
    }

    [HttpGet("/UserManagement/Edit/{id}")]
    public async Task<IActionResult> Edit(long id)
    {
        var user = await _db.Users
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for edit.", id);
            return NotFound();
        }

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

        _logger.LogInformation(
            "Edit page opened for user {UserId} by {Admin}.",
            id,
            User.Identity?.Name ?? "Unknown");

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

            _logger.LogWarning(
                "Invalid user edit submitted for {UserId} by {Admin}.",
                model.UserId,
                User.Identity?.Name ?? "Unknown");

            return View(model);
        }

        try
        {
            var user = await _db.Users
                .Include(x => x.Permissions)
                .FirstOrDefaultAsync(x => x.UserId == model.UserId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for update.", model.UserId);
                return NotFound();
            }

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

            _logger.LogInformation(
                "User {UserId} updated by {Admin}.",
                model.UserId,
                User.Identity?.Name ?? "Unknown");

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while updating user {UserId}.",
                model.UserId);

            throw;
        }
    }

    [HttpGet("/UserManagement/ResetPassword/{id}")]
    public async Task<IActionResult> ResetPassword(long id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for password reset page.", id);
            return NotFound();
        }

        var model = new UserResetPasswordViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName
        };

        _logger.LogInformation(
            "Reset password page opened for user {UserId} by {Admin}.",
            id,
            User.Identity?.Name ?? "Unknown");

        return View(model);
    }

    [HttpPost("/UserManagement/ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(UserResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Invalid password reset submitted for user {UserId}.",
                model.UserId);

            return View(model);
        }

        try
        {
            var user = await _db.Users.FindAsync(model.UserId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for password reset.", model.UserId);
                return NotFound();
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Password reset completed for user {UserId} by {Admin}.",
                model.UserId,
                User.Identity?.Name ?? "Unknown");

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while resetting password for user {UserId}.",
                model.UserId);

            throw;
        }
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

            _logger.LogInformation(
                "User {UserId} activated by {Admin}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }
        else
        {
            _logger.LogWarning("User {UserId} not found for activation.", id);
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

            _logger.LogInformation(
                "User {UserId} deactivated by {Admin}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }
        else
        {
            _logger.LogWarning("User {UserId} not found for deactivation.", id);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("/UserManagement/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _db.Users
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.UserId == id);

        if (user != null)
        {
            _db.UserPermissions.RemoveRange(user.Permissions);
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} deleted by {Admin}.",
                id,
                User.Identity?.Name ?? "Unknown");
        }
        else
        {
            _logger.LogWarning("User {UserId} not found for deletion.", id);
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
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}