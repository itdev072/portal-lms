using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public AuthController(AppData data) : base(data) { }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = Data.Users.Where(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        await SignInAsync(user);
        return Ok(UserDto.From(user));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            return BadRequest(new { message = "Email and a password of at least 8 characters are required." });

        if (Data.Users.Where(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)).Any())
            return BadRequest(new { message = "An account with that email already exists." });

        var user = new ApplicationUser
        {
            Email = req.Email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? req.Email : req.DisplayName.Trim(),
            Role = UserRole.Student // public registration is always Student, per product spec
        };
        user.PasswordHash = _hasher.HashPassword(user, req.Password);
        Data.Users.Upsert(user);

        await SignInAsync(user);
        return Ok(UserDto.From(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();
        return Ok(UserDto.From(user));
    }

    private async Task SignInAsync(ApplicationUser user)
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
}
