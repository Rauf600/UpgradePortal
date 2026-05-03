using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
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

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}