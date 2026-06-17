using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public const string DefaultCurrency = "VND";

    private Money()
    {
        // Required by EF Core
    }

    private Money(long amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Money amount cannot be negative."
            );

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        Amount = amount;
        Currency = NormalizeCurrency(currency);
    }

    /// <summary>
    /// Amount được lưu bằng đơn vị nhỏ nhất.
    /// Với VND: 120000 nghĩa là 120.000₫.
    /// Với USD nếu dùng cents: 1234 nghĩa là $12.34.
    /// </summary>
    public long Amount { get; private set; }

    public string Currency { get; private set; } = DefaultCurrency;

    public bool IsZero => Amount == 0;

    public static Money Zero(string currency = DefaultCurrency)
    {
        return new Money(0, currency);
    }

    public bool IsNegative()
    {
        return Amount < 0;
    }

    public static Money Create(long amount, string currency = DefaultCurrency)
    {
        return new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);

        return Create(checked(Amount + other.Amount), Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        if (other.Amount > Amount)
            throw new InvalidOperationException("Money amount cannot be negative.");

        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero."
            );

        return Create(checked(Amount * quantity), Currency);
    }

    public Money MultiplyByRate(decimal rate)
    {
        if (rate < 0)
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate cannot be negative.");

        var amount = (long)Math.Round(Amount * rate, MidpointRounding.AwayFromZero);

        return Create(amount, Currency);
    }

    public Money ApplyPercentageDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                "Percentage must be between 0 and 100."
            );

        var rate = 1 - percentage / 100m;

        return MultiplyByRate(rate);
    }

    public Money ApplyDiscountAmount(Money discountAmount)
    {
        EnsureSameCurrency(discountAmount);

        if (discountAmount.Amount > Amount)
            return Zero(Currency);

        return Subtract(discountAmount);
    }

    public Money ApplyDiscountRate(Percentage discountRate)
    {
        ArgumentNullException.ThrowIfNull(discountRate);
        checked
        {
            var discountedAmount =
                Amount * (Percentage.Scale - discountRate.BasisPoints) / Percentage.Scale;

            return Create(discountedAmount, Currency);
        }
    }

    public Money CapAt(Money maximum)
    {
        EnsureSameCurrency(maximum);

        return Amount > maximum.Amount ? maximum : this;
    }

    public Money Min(Money other)
    {
        EnsureSameCurrency(other);

        return Amount <= other.Amount ? this : other;
    }

    public Money Max(Money other)
    {
        EnsureSameCurrency(other);

        return Amount >= other.Amount ? this : other;
    }

    public bool GreaterThan(Money other)
    {
        EnsureSameCurrency(other);

        return Amount > other.Amount;
    }

    public bool GreaterThanOrEqual(Money other)
    {
        EnsureSameCurrency(other);

        return Amount >= other.Amount;
    }

    public bool LessThan(Money other)
    {
        EnsureSameCurrency(other);

        return Amount < other.Amount;
    }

    public Money PercentageDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                "Percentage must be between 0 and 100."
            );

        var discount = (long)Math.Round(Amount * percentage / 100m, MidpointRounding.AwayFromZero);

        return Create(discount, Currency);
    }

    public Money AddShippingFee(Money shippingFee)
    {
        return Add(shippingFee);
    }

    public Money AddTax(decimal taxRatePercentage)
    {
        if (taxRatePercentage < 0)
            throw new ArgumentOutOfRangeException(
                nameof(taxRatePercentage),
                "Tax rate cannot be negative."
            );

        var tax = (long)
            Math.Round(Amount * taxRatePercentage / 100m, MidpointRounding.AwayFromZero);

        return Add(Create(tax, Currency));
    }

    public override string ToString()
    {
        return Currency == DefaultCurrency ? $"{Amount:N0} ₫" : $"{Amount} {Currency}";
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies. Left: {Currency}, Right: {other.Currency}."
            );
    }

    private static string NormalizeCurrency(string currency)
    {
        currency = currency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
            throw new ArgumentException(
                "Currency must be ISO 4217 format, for example VND, USD, EUR.",
                nameof(currency)
            );

        return currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
