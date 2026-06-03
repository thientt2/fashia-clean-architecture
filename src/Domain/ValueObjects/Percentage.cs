using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public sealed class Percentage : ValueObject
{
    private Percentage(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static Percentage Create(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Percentage must be between 0 and 100.");

        return new Percentage(decimal.Round(value, 2, MidpointRounding.AwayFromZero));
    }

    public decimal ToDecimal()
    {
        return Value / 100m;
    }

    public static Percentage Zero => new(0);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
