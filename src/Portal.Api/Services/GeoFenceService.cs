namespace Portal.Api.Services;

/// <summary>
/// Pure math, no external dependency: the haversine formula for great-circle distance
/// between two lat/lng points, in meters. Used to check a student's check-in location
/// against the campus location's allowed radius.
/// </summary>
public class GeoFenceService
{
    private const double EarthRadiusMeters = 6_371_000;

    public double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    public bool IsWithinRadius(double lat1, double lon1, double lat2, double lon2, int radiusMeters)
        => DistanceMeters(lat1, lon1, lat2, lon2) <= radiusMeters;

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
