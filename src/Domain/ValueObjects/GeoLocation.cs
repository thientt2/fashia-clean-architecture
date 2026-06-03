namespace Fashia.Domain.ValueObjects;

public sealed class GeoLocation : ValueObject
{
    private GeoLocation(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public decimal Latitude { get; }

    public decimal Longitude { get; }

    public static GeoLocation Create(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90.");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180.");

        return new GeoLocation(latitude, longitude);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
