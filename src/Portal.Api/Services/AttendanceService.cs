using Portal.Api.Data;
using Portal.Api.Models.Domain;

namespace Portal.Api.Services;

public class AttendanceService
{
    private readonly AppData _data;
    private readonly GeoFenceService _geo;

    public AttendanceService(AppData data, GeoFenceService geo)
    {
        _data = data;
        _geo = geo;
    }

    public AttendanceSession CreateSession(string classSectionId, string teacherId, string campusLocationId, int validMinutes = 10)
    {
        var session = new AttendanceSession
        {
            ClassSectionId = classSectionId,
            TeacherId = teacherId,
            CampusLocationId = campusLocationId,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(validMinutes)
        };
        _data.AttendanceSessions.Upsert(session);
        return session;
    }

    /// <summary>
    /// The core of QR + geofenced attendance: the QR code only hands the student's phone
    /// a URL containing the session token (see wwwroot/js/attendance-teacher.js +
    /// Views/Teacher/AttendanceSession.cshtml) — getting that far proves nothing. This
    /// method is what actually decides "present": the session must still be open, AND the
    /// phone's own browser-reported GPS coordinates (captured client-side via
    /// navigator.geolocation in wwwroot/js/attendance-student.js, never trusted from a
    /// hidden field) must fall within the campus location's configured radius.
    /// </summary>
    public AttendanceRecord CheckIn(string token, string studentId, double lat, double lng)
    {
        var session = _data.AttendanceSessions.Where(s => s.Token == token).FirstOrDefault();

        if (session == null)
        {
            return Record(studentId, "", "", lat, lng, 0, AttendanceStatus.InvalidSession, AttendanceSource.Qr);
        }

        if (session.IsClosed || DateTimeOffset.UtcNow > session.ExpiresAt)
        {
            return Record(studentId, session.Id, session.ClassSectionId, lat, lng, 0, AttendanceStatus.Expired, AttendanceSource.Qr);
        }

        var campus = _data.CampusLocations.Find(session.CampusLocationId);
        if (campus == null)
        {
            return Record(studentId, session.Id, session.ClassSectionId, lat, lng, 0, AttendanceStatus.InvalidSession, AttendanceSource.Qr);
        }

        var distance = _geo.DistanceMeters(lat, lng, campus.Latitude, campus.Longitude);
        var status = distance <= campus.RadiusMeters ? AttendanceStatus.Present : AttendanceStatus.OutOfRange;

        return Record(studentId, session.Id, session.ClassSectionId, lat, lng, distance, status, AttendanceSource.Qr);
    }

    /// <summary>Teacher ticks a student present/absent/late by hand — no geolocation involved.</summary>
    public AttendanceRecord MarkManual(string classSectionId, string studentId, AttendanceStatus status, DateOnly date)
    {
        var existing = _data.AttendanceRecords
            .Where(r => r.ClassSectionId == classSectionId && r.StudentId == studentId && r.Date == date)
            .FirstOrDefault();

        var record = existing ?? new AttendanceRecord
        {
            ClassSectionId = classSectionId,
            StudentId = studentId,
            Date = date
        };
        record.Status = status;
        record.Source = AttendanceSource.Manual;
        record.ScannedAt = DateTimeOffset.UtcNow;
        _data.AttendanceRecords.Upsert(record);
        return record;
    }

    private AttendanceRecord Record(string studentId, string sessionId, string classSectionId, double lat, double lng, double distance, AttendanceStatus status, AttendanceSource source)
    {
        var record = new AttendanceRecord
        {
            SessionId = sessionId,
            ClassSectionId = classSectionId,
            StudentId = studentId,
            Latitude = lat,
            Longitude = lng,
            DistanceMeters = Math.Round(distance, 1),
            Status = status,
            Source = source
        };
        _data.AttendanceRecords.Upsert(record);
        return record;
    }
}
