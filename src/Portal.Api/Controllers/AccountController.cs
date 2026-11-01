using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

/// <summary>Any signed-in user manages their own profile picture + preferences here
/// (distinct from Admin's UsersController, which manages OTHER users' accounts).</summary>
[Route("api/account")]
[Authorize]
public class AccountController : ApiControllerBase
{
    public AccountController(AppData data) : base(data) { }

    [HttpGet]
    public IActionResult Get()
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();
        return Ok(AccountDto.From(user));
    }

    [HttpPut]
    public IActionResult Update(UpdateAccountRequest req)
    {
        var user = CurrentUser;
        if (user == null) return Unauthorized();

        if (!string.IsNullOrWhiteSpace(req.DisplayName)) user.DisplayName = req.DisplayName.Trim();
        if (req.ProfilePictureDataUrl != null) user.ProfilePictureDataUrl = req.ProfilePictureDataUrl;
        if (req.Preferences != null) user.Preferences = req.Preferences;

        Data.Users.Upsert(user);
        return Ok(AccountDto.From(user));
    }
}
