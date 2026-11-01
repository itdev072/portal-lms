using Portal.Web.Data;
using Portal.Web.Models.Domain;

namespace Portal.Web.Services;

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
            return Record(studentId, "", lat, lng, 0, AttendanceStatus.InvalidSession);
        }

        if (session.IsClosed || DateTimeOffset.UtcNow > session.ExpiresAt)
        {
            return Record(studentId, session.Id, lat, lng, 0, AttendanceStatus.Expired);
        }

        var campus = _data.CampusLocations.Find(session.CampusLocationId);
        if (campus == null)
        {
            return Record(studentId, session.Id, lat, lng, 0, AttendanceStatus.InvalidSession);
        }

        var distance = _geo.DistanceMeters(lat, lng, campus.Latitude, campus.Longitude);
        var status = distance <= campus.RadiusMeters ? AttendanceStatus.Present : AttendanceStatus.OutOfRange;

        return Record(studentId, session.Id, lat, lng, distance, status);
    }

    private AttendanceRecord Record(string studentId, string sessionId, double lat, double lng, double distance, AttendanceStatus status)
    {
        var record = new AttendanceRecord
        {
            SessionId = sessionId,
            StudentId = studentId,
            Latitude = lat,
            Longitude = lng,
            DistanceMeters = Math.Round(distance, 1),
            Status = status
        };
        _data.AttendanceRecords.Upsert(record);
        return record;
    }
}
