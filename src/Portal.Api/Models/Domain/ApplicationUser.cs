namespace Portal.Api.Models.Domain;

/// <summary>
/// The Portal's own user record. We don't depend on EF Core / ASP.NET Core Identity's
/// database-backed stores here (see README: "Why no EF Core") — just a plain POCO
/// persisted through JsonStore, with ASP.NET Core's built-in PasswordHasher&lt;T&gt;
/// for secure password hashing and cookie authentication for the session.
/// </summary>
public class ApplicationUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>A small profile photo, stored inline as a data: URL (no blob storage available) — fine at avatar size, not for large images.</summary>
    public string? ProfilePictureDataUrl { get; set; }

    public UserPreferences Preferences { get; set; } = new();
}

public class UserPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public string Theme { get; set; } = "dark"; // "dark" | "light"
    public string Language { get; set; } = "en"; // "en" | "km"
}

/// <summary>A single-row collection: platform-wide settings an Admin configures.</summary>
public class PlatformSettings
{
    public string Id { get; set; } = "singleton";
    public string SiteName { get; set; } = "Portal";
    public string SupportEmail { get; set; } = "support@portal.local";
    public bool AllowPublicRegistration { get; set; } = true;
    public int DefaultAttendanceRadiusMeters { get; set; } = 100;
}
