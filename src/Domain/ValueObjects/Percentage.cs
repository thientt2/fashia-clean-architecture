using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public sealed class Percentage : ValueObject
{
    public const int Scale = 10_000;

    public int BasisPoints { get; }

    private Percentage(int basisPoints)
    {
        if (basisPoints is < 0 or > Scale)
            throw new ArgumentOutOfRangeException(
                nameof(basisPoints),
                "Percentage must be between 0% and 100%."
            );

        BasisPoints = basisPoints;
    }

    public static Percentage FromPercentage(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                "Percentage must be between 0 and 100."
            );

        var basisPoints = (int)decimal.Round(percentage * 100m, 0, MidpointRounding.AwayFromZero);

        return new Percentage(basisPoints);
    }

    public static Percentage FromBasisPoints(int basisPoints) => new(basisPoints);

    public decimal ToPercentage() => BasisPoints / 100m;

    public decimal ToRatio() => BasisPoints / (decimal)Scale;

    public static Percentage Zero => new(0);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BasisPoints;
    }
}
