using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public Money() { }

    private Money(long amount)
    {
        Amount = amount;
    }

    public long Amount { get; init; }

    public static Money Create(long amount)
    {
        if (amount < 0)
            throw new ArgumentException("Money cannot be negative.");

        return new Money(amount);
    }

    public static Money Zero => new(0);

    public Money Add(Money other)
    {
        return Create(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        if (other.Amount > Amount)
            throw new InvalidOperationException("Money cannot be negative.");

        return Create(Amount - other.Amount);
    }

    public Money Multiply(decimal factor)
    {
        var result = (long)Math.Round(Amount * factor, MidpointRounding.AwayFromZero);
        return Create(result);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}
