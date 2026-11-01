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
}
