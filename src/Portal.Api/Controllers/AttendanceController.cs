using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Api.Data;
using Portal.Api.Models.Domain;
using Portal.Api.Models.Dto;
using Portal.Api.Services;

namespace Portal.Api.Controllers;

[Route("api")]
public class AttendanceController : ApiControllerBase
{
    private readonly AttendanceService _attendance;

    public AttendanceController(AppData data, AttendanceService attendance) : base(data)
    {
        _attendance = attendance;
    }

    /// <summary>Teacher opens a QR check-in window for a class.</summary>
    [HttpPost("teacher/attendance/sessions")]
    [Authorize(Roles = "Teacher")]
    public IActionResult OpenSession([FromBody] OpenSessionRequest req)
    {
        var cls = Data.ClassSections.Find(req.ClassSectionId);
        if (cls == null || cls.TeacherId != CurrentUserId) return Forbidden();

        var session = _attendance.CreateSession(cls.Id, CurrentUserId!, cls.CampusLocationId, req.ValidMinutes ?? 10);
        return Ok(new { session.Id, session.Token, session.ExpiresAt, checkInPath = $"/attendance/check-in/{session.Token}" });
    }

    [HttpPost("teacher/attendance/sessions/{id}/close")]
    [Authorize(Roles = "Teacher")]
    public IActionResult CloseSession(string id)
    {
        var session = Data.AttendanceSessions.Find(id);
        if (session == null || session.TeacherId != CurrentUserId) return Forbidden();
        session.IsClosed = true;
        Data.AttendanceSessions.Upsert(session);
        return Ok();
    }

    /// <summary>Student's phone posts its GPS position after scanning the QR.</summary>
    [HttpPost("attendance/check-in/{token}")]
    [Authorize(Roles = "Student")]
    public IActionResult CheckIn(string token, [FromBody] CheckInRequest req)
    {
        var record = _attendance.CheckIn(token, CurrentUserId!, req.Latitude, req.Longitude);
        return Ok(new { record.Status, record.DistanceMeters });
    }

    /// <summary>Teacher's roster for a class + date, with each student's current status (manual or QR).</summary>
    [HttpGet("teacher/classes/{classId}/attendance")]
    [Authorize(Roles = "Teacher,Admin")]
    public IActionResult RosterForDate(string classId, [FromQuery] DateOnly? date)
    {
        var cls = Data.ClassSections.Find(classId);
        if (cls == null) return NotFound();
        if (CurrentRole == UserRole.Teacher && cls.TeacherId != CurrentUserId) return Forbidden();

        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var studentIds = Data.Enrollments.Where(e => e.ClassSectionId == classId).Select(e => e.StudentId).ToHashSet();
        var records = Data.AttendanceRecords.Where(r => r.ClassSectionId == classId && r.Date == d)
            .ToDictionary(r => r.StudentId, r => r);

        var roster = Data.Users.Where(u => studentIds.Contains(u.Id)).OrderBy(u => u.DisplayName)
            .Select(u => new AttendanceRosterEntry(
                u.Id, u.DisplayName,
                records.TryGetValue(u.Id, out var rec) ? rec.Status : null,
                records.TryGetValue(u.Id, out var rec2) ? rec2.Source.ToString() : ""));

        return Ok(roster);
    }

    /// <summary>Tick a student present/absent/late by hand.</summary>
    [HttpPost("teacher/classes/{classId}/attendance")]
    [Authorize(Roles = "Teacher,Admin")]
    public IActionResult MarkManual(string classId, MarkAttendanceRequest req)
    {
        var cls = Data.ClassSections.Find(classId);
        if (cls == null) return NotFound();
        if (CurrentRole == UserRole.Teacher && cls.TeacherId != CurrentUserId) return Forbidden();

        var date = req.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var record = _attendance.MarkManual(classId, req.StudentId, req.Status, date);
        return Ok(record);
    }

    /// <summary>A student's own attendance history, across all their classes.</summary>
    [HttpGet("student/attendance")]
    [Authorize(Roles = "Student")]
    public IActionResult MyAttendance()
    {
        var records = Data.AttendanceRecords.Where(r => r.StudentId == CurrentUserId)
            .OrderByDescending(r => r.Date)
            .Select(r => new { r.ClassSectionId, r.Date, r.Status, r.Source });
        return Ok(records);
    }
}

public record OpenSessionRequest(string ClassSectionId, int? ValidMinutes);
public record CheckInRequest(double Latitude, double Longitude);
