using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ApiControllerBase
{
    private readonly PasswordHasher<ApplicationUser> _hasher = new();

    public UsersController(AppData data) : base(data) { }

    [HttpGet]
    public IActionResult List([FromQuery] UserRole? role)
    {
        var users = Data.Users.All().AsEnumerable();
        if (role.HasValue) users = users.Where(u => u.Role == role.Value);
        return Ok(users.OrderBy(u => u.DisplayName).Select(UserDto.From));
    }

    [HttpPost]
    public IActionResult Create(CreateUserRequest req)
    {
        if (Data.Users.Where(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)).Any())
            return BadRequest(new { message = "An account with that email already exists." });

        var user = new ApplicationUser { Email = req.Email.Trim(), DisplayName = req.DisplayName.Trim(), Role = req.Role };
        user.PasswordHash = _hasher.HashPassword(user, req.Password);
        Data.Users.Upsert(user);
        return Ok(UserDto.From(user));
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, UpdateUserRequest req)
    {
        var user = Data.Users.Find(id);
        if (user == null) return NotFound();
        user.DisplayName = req.DisplayName;
        user.Role = req.Role;
        user.IsActive = req.IsActive;
        Data.Users.Upsert(user);
        return Ok(UserDto.From(user));
    }

    [HttpDelete("{id}")]
    public IActionResult Deactivate(string id)
    {
        var user = Data.Users.Find(id);
        if (user == null) return NotFound();
        user.IsActive = false;
        Data.Users.Upsert(user);
        return Ok();
    }
}
