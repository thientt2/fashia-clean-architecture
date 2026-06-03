namespace Fashia.Domain.Entities;

public class OrderVoucher : BaseAuditableEntity
{
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public int VoucherId { get; private set; }
    public Voucher Voucher { get; private set; } = null!;

    public string VoucherCode { get; private set; } = string.Empty;

    public decimal DiscountAmount { get; private set; }

    public DateTime AppliedAt { get; private set; }

    private OrderVoucher()
    {
        // EF Core
    }

    public OrderVoucher(int voucherId, string voucherCode, decimal discountAmount)
    {
        SetVoucherId(voucherId);
        SetVoucherCode(voucherCode);
        SetDiscountAmount(discountAmount);
        AppliedAt = DateTime.UtcNow;
    }

    private void SetVoucherId(int voucherId)
    {
        if (voucherId <= 0)
            throw new ArgumentException("Voucher id is required.", nameof(voucherId));

        VoucherId = voucherId;
    }

    private void SetVoucherCode(string voucherCode)
    {
        if (string.IsNullOrWhiteSpace(voucherCode))
            throw new ArgumentException("Voucher code is required.", nameof(voucherCode));

        VoucherCode = voucherCode.Trim().ToUpperInvariant();
    }

    private void SetDiscountAmount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));

        DiscountAmount = decimal.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
    }
}
