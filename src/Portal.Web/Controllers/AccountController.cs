using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portal.Web.Data;
using Portal.Web.Models.Domain;
using Portal.Web.Models.ViewModels;

namespace Portal.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppData _data;
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public AccountController(AppData data)
    {
        _data = data;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = _data.Users.Where(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

        if (user == null || !user.IsActive ||
            _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            model.Error = "Invalid email or password.";
            return View(model);
        }

        await SignInUserAsync(user);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToDashboard(user.Role);
    }

    /// <summary>
    /// Self-service registration always creates a Student account (see the spec's P0
    /// requirement: "Teacher and Admin accounts are created by an Admin, not
    /// self-registered").
    /// </summary>
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (_data.Users.Where(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)).Any())
        {
            model.Error = "An account with this email already exists.";
            return View(model);
        }

        var user = new ApplicationUser
        {
            Email = model.Email,
            DisplayName = model.DisplayName,
            Role = UserRole.Student
        };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        _data.Users.Upsert(user);

        await SignInUserAsync(user);
        return RedirectToDashboard(user.Role);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SignInUserAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    private IActionResult RedirectToDashboard(UserRole role) => role switch
    {
        UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
        UserRole.Teacher => RedirectToAction("Dashboard", "Teacher"),
        _ => RedirectToAction("Dashboard", "Student")
    };
}
