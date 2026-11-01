using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Web.Data;
using Portal.Web.Models.Domain;
using Portal.Web.Models.ViewModels;
using Portal.Web.Services;

namespace Portal.Web.Controllers;

/// <summary>
/// What a student's phone hits after scanning the teacher's QR code. See
/// AttendanceService.CheckIn for the actual present/out-of-range decision — this
/// controller only renders the page (which asks the browser for its own GPS location)
/// and relays what the browser reports to the service.
/// </summary>
[Authorize(Roles = "Student")]
public class AttendanceController : Controller
{
    private readonly AppData _data;
    private readonly AttendanceService _attendance;

    public AttendanceController(AppData data, AttendanceService attendance)
    {
        _data = data;
        _attendance = attendance;
    }

    [HttpGet("/attend/{token}")]
    public IActionResult CheckIn(string token)
    {
        var session = _data.AttendanceSessions.Where(s => s.Token == token).FirstOrDefault();
        var section = session != null ? _data.ClassSections.Find(session.ClassSectionId) : null;

        return View(new AttendanceCheckInViewModel
        {
            Token = token,
            ClassName = section?.Name ?? "Unknown session"
        });
    }

    [HttpPost("/attend/{token}"), ValidateAntiForgeryToken]
    public IActionResult Confirm(string token, double latitude, double longitude)
    {
        var studentId = User.GetUserId()!;
        var record = _attendance.CheckIn(token, studentId, latitude, longitude);

        var session = _data.AttendanceSessions.Where(s => s.Token == token).FirstOrDefault();
        var campus = session != null ? _data.CampusLocations.Find(session.CampusLocationId) : null;
        var section = session != null ? _data.ClassSections.Find(session.ClassSectionId) : null;

        return View("CheckIn", new AttendanceCheckInViewModel
        {
            Token = token,
            ClassName = section?.Name ?? "Unknown session",
            Result = record.Status,
            DistanceMeters = record.DistanceMeters,
            RadiusMeters = campus?.RadiusMeters
        });
    }
}
