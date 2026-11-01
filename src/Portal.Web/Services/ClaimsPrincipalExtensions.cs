using System.Security.Claims;
using Portal.Web.Models.Domain;

namespace Portal.Web.Services;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static string? GetDisplayName(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Name);

    public static UserRole? GetRole(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.Role);
        return raw != null && Enum.TryParse<UserRole>(raw, out var role) ? role : null;
    }
}
