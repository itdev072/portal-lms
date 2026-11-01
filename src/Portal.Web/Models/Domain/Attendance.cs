namespace Portal.Web.Models.Domain;

/// <summary>
/// A fixed point the campus trusts (e.g. "Room 204" or "Main Campus Gate") with a
/// radius, in meters, inside which a student's check-in counts as physically present.
/// </summary>
public class CampusLocation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; } = 100;
}

/// <summary>
/// One roll call. The teacher opens a session for a class; the Portal renders it as a
/// QR code (drawn client-side, see wwwroot/js/attendance-teacher.js) that encodes a URL
/// containing <see cref="Token"/>. The QR itself never grants attendance — the token
/// only lets the student's phone reach the check-in page, which then asks for the
/// phone's own location and the server verifies that location server-side
/// (see GeoFenceService) before recording them present. The session expires quickly so a
/// photographed QR can't be reused later or from off campus.
/// </summary>
public class AttendanceSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ClassSectionId { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public string CampusLocationId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsClosed { get; set; }
}

public class AttendanceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string SessionId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public DateTimeOffset ScannedAt { get; set; } = DateTimeOffset.UtcNow;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceMeters { get; set; }
    public AttendanceStatus Status { get; set; }
}
