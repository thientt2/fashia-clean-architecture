namespace Fashia.Domain.Entities;

public class OrderVoucher : BaseAuditableEntity
{
    public int OrderId { get; private set; }

    public int VoucherId { get; private set; }

    public string VoucherCode { get; private set; } = string.Empty;

    public DiscountType DiscountType { get; private set; }

    public int DiscountValue { get; private set; }

    public Money DiscountAmount { get; private set; } = null!;

    public DateTime AppliedAt { get; private set; }

    private OrderVoucher()
    {
        // EF Core
    }

    public OrderVoucher(int voucherId, string voucherCode, Money discountAmount)
        : this(voucherId, voucherCode, DiscountType.FixedAmount, (int)discountAmount.Amount, discountAmount)
    {
    }

    public OrderVoucher(
        int voucherId,
        string voucherCode,
        DiscountType discountType,
        int discountValue,
        Money discountAmount
    )
    {
        if (voucherId <= 0)
            throw new ArgumentException("Voucher id is required.", nameof(voucherId));

        if (string.IsNullOrWhiteSpace(voucherCode))
            throw new ArgumentException("Voucher code is required.", nameof(voucherCode));

        if (!Enum.IsDefined(typeof(DiscountType), discountType))
            throw new ArgumentException("Invalid discount type.", nameof(discountType));

        if (discountValue <= 0)
            throw new ArgumentException("Discount value must be greater than zero.", nameof(discountValue));

        if (discountAmount.IsNegative())
            throw new ArgumentException(
                "Discount amount cannot be negative.",
                nameof(discountAmount)
            );

        VoucherId = voucherId;
        VoucherCode = voucherCode;
        DiscountType = discountType;
        DiscountValue = discountValue;
        DiscountAmount = discountAmount;
        AppliedAt = DateTime.UtcNow;
    }
}
