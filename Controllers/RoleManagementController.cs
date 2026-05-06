using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpgradePortal.Web.Data;
using UpgradePortal.Web.Models;
using UpgradePortal.Web.ViewModels;

namespace UpgradePortal.Web.Controllers;

[Authorize]
public class RoleManagementController : Controller
{
    private readonly AppDbContext _db;

    public RoleManagementController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("/RoleManagement")]
    public async Task<IActionResult> Index()
    {
        var roles = await _db.Roles
            .OrderBy(x => x.RoleName)
            .ToListAsync();

        return View(roles);
    }

    [HttpGet("/RoleManagement/Create")]
    public IActionResult Create()
    {
        return View(new RoleViewModel());
    }

    [HttpPost("/RoleManagement/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel model)
    {
        if (await _db.Roles.AnyAsync(x => x.RoleName == model.RoleName))
        {
            ModelState.AddModelError(nameof(model.RoleName), "This role already exists.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var role = new Role
        {
            RoleName = model.RoleName,
            Description = model.Description
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
    [HttpPost("/RoleManagement/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var role = await _db.Roles.FindAsync(id);

        if (role == null)
            return NotFound();

        var roleInUse = await _db.Users.AnyAsync(x => x.RoleId == id);

        if (roleInUse)
        {
            TempData["Error"] = "This role cannot be deleted because it is assigned to one or more users.";
            return RedirectToAction("Index");
        }

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet("/RoleManagement/Edit/{id}")]
    public async Task<IActionResult> Edit(long id)
    {
        var role = await _db.Roles.FindAsync(id);

        if (role == null)
            return NotFound();

        var model = new RoleViewModel
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description
        };

        return View(model);
    }

    [HttpPost("/RoleManagement/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var role = await _db.Roles.FindAsync(model.RoleId);

        if (role == null)
            return NotFound();

        role.RoleName = model.RoleName;
        role.Description = model.Description;

        await _db.SaveChangesAsync();

        return RedirectToAction("Index");

    }
}