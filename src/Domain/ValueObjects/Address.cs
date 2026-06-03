namespace Fashia.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Line1 { get; private set; } = string.Empty;
    public string Ward { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public string Province { get; private set; } = string.Empty;

    private Address()
    {
        // EF Core
    }

    public Address(string line1, string ward, string district, string province)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Address line 1 cannot be empty.", nameof(line1));
        if (string.IsNullOrWhiteSpace(ward))
            throw new ArgumentException("Ward cannot be empty.", nameof(ward));
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District cannot be empty.", nameof(district));
        if (string.IsNullOrWhiteSpace(province))
            throw new ArgumentException("Province cannot be empty.", nameof(province));

        Line1 = line1;
        Ward = ward;
        District = district;
        Province = province;
    }

    public static Address Create(string line1, string ward, string district, string province)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Address line 1 cannot be empty.", nameof(line1));
        if (string.IsNullOrWhiteSpace(ward))
            throw new ArgumentException("Ward cannot be empty.", nameof(ward));
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District cannot be empty.", nameof(district));
        if (string.IsNullOrWhiteSpace(province))
            throw new ArgumentException("Province cannot be empty.", nameof(province));

        return new Address(line1, ward, district, province);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Line1;
        yield return Ward;
        yield return District;
        yield return Province;
    }
}
