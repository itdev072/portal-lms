using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;

namespace Portal.Api.Controllers;

[Route("api/admin/settings")]
[Authorize(Roles = "Admin")]
public class SettingsController : ApiControllerBase
{
    public SettingsController(AppData data) : base(data) { }

    private PlatformSettings Current()
    {
        var s = Data.Settings.Find("singleton");
        if (s == null)
        {
            s = new PlatformSettings();
            Data.Settings.Upsert(s);
        }
        return s;
    }

    [HttpGet]
    public IActionResult Get() => Ok(SettingsDto.From(Current()));

    [HttpPut]
    public IActionResult Update(SettingsSaveRequest req)
    {
        var s = Current();
        s.SiteName = req.SiteName;
        s.SupportEmail = req.SupportEmail;
        s.AllowPublicRegistration = req.AllowPublicRegistration;
        s.DefaultAttendanceRadiusMeters = req.DefaultAttendanceRadiusMeters;
        Data.Settings.Upsert(s);
        return Ok(SettingsDto.From(s));
    }
}
