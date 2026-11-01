using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portal.Web.Data;
using Portal.Web.Models.Domain;
using Portal.Web.Models.ViewModels;

namespace Portal.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppData _data;
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public AdminController(AppData data)
    {
        _data = data;
    }

    public IActionResult Dashboard()
    {
        var users = _data.Users.All();
        return View(new AdminDashboardViewModel
        {
            StudentCount = users.Count(u => u.Role == UserRole.Student),
            TeacherCount = users.Count(u => u.Role == UserRole.Teacher),
            LevelCount = _data.Levels.All().Count,
            ActiveClassSections = _data.ClassSections.All().Count
        });
    }

    public IActionResult Users() => View(_data.Users.All().OrderBy(u => u.Role).ThenBy(u => u.DisplayName).ToList());

    [HttpGet]
    public IActionResult CreateUser() => View(new CreateUserViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateUser(CreateUserViewModel model)
    {
        if (_data.Users.Where(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)).Any())
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser { Email = model.Email, DisplayName = model.DisplayName, Role = model.Role };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        _data.Users.Upsert(user);

        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ToggleActive(string id)
    {
        var user = _data.Users.Find(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        _data.Users.Upsert(user);
        return RedirectToAction(nameof(Users));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ChangeRole(string id, UserRole role)
    {
        var user = _data.Users.Find(id);
        if (user == null) return NotFound();
        user.Role = role;
        _data.Users.Upsert(user);
        return RedirectToAction(nameof(Users));
    }

    public IActionResult Levels() => View(_data.Levels.All().OrderBy(l => l.Order).ToList());

    [HttpGet]
    public IActionResult CreateLevel() => View("EditLevel", new LevelFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult CreateLevel(LevelFormViewModel model)
    {
        _data.Levels.Upsert(model.ToDomain());
        return RedirectToAction(nameof(Levels));
    }

    [HttpGet]
    public IActionResult EditLevel(string id)
    {
        var level = _data.Levels.Find(id);
        return level == null ? NotFound() : View(LevelFormViewModel.FromDomain(level));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult EditLevel(LevelFormViewModel model)
    {
        _data.Levels.Upsert(model.ToDomain());
        return RedirectToAction(nameof(Levels));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult DeleteLevel(string id)
    {
        _data.Levels.Delete(id);
        return RedirectToAction(nameof(Levels));
    }
}
