using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Services;

namespace Portal.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected AppData Data { get; }

    protected ApiControllerBase(AppData data)
    {
        Data = data;
    }

    protected string? CurrentUserId => User.GetUserId();
    protected UserRole? CurrentRole => User.GetRole();

    protected ApplicationUser? CurrentUser =>
        CurrentUserId is null ? null : Data.Users.Find(CurrentUserId);

    protected IActionResult Forbidden(string message = "You don't have permission to do that.")
        => StatusCode(403, new { message });
}
