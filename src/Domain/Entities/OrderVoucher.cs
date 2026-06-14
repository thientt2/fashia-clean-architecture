namespace Fashia.Domain.Entities;

public class OrderVoucher : BaseAuditableEntity
{
    public int OrderId { get; private set; }

    public int VoucherId { get; private set; }

    public string VoucherCode { get; private set; } = string.Empty;

    public Money DiscountAmount { get; private set; } = null!;

    public DateTime AppliedAt { get; private set; }

    private OrderVoucher()
    {
        // EF Core
    }

    public OrderVoucher(int voucherId, string voucherCode, Money discountAmount)
    {
        if (voucherId <= 0)
            throw new ArgumentException("Voucher id is required.", nameof(voucherId));

        if (string.IsNullOrWhiteSpace(voucherCode))
            throw new ArgumentException("Voucher code is required.", nameof(voucherCode));

        if (discountAmount.IsNegative())
            throw new ArgumentException(
                "Discount amount cannot be negative.",
                nameof(discountAmount)
            );

        VoucherId = voucherId;
        VoucherCode = voucherCode;
        DiscountAmount = discountAmount;
        AppliedAt = DateTime.UtcNow;
    }
}
