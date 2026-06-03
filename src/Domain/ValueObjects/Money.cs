using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    private Money(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }

    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Money cannot be negative.");

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero));
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
        return Create(Amount * factor);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}
