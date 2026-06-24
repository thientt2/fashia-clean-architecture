namespace Fashia.Application.Orders.Commands.PlaceOrder;

internal static class HaversineDistance
{
    private const double EarthRadiusKilometers = 6371.0088;

    internal static double Kilometers(
        decimal originLatitude,
        decimal originLongitude,
        decimal destinationLatitude,
        decimal destinationLongitude
    )
    {
        var originLatitudeRadians = ToRadians((double)originLatitude);
        var destinationLatitudeRadians = ToRadians((double)destinationLatitude);
        var latitudeDelta = ToRadians((double)(destinationLatitude - originLatitude));
        var longitudeDelta = ToRadians((double)(destinationLongitude - originLongitude));

        var haversine =
            Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(originLatitudeRadians)
                * Math.Cos(destinationLatitudeRadians)
                * Math.Sin(longitudeDelta / 2)
                * Math.Sin(longitudeDelta / 2);

        var centralAngle = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));

        return EarthRadiusKilometers * centralAngle;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
