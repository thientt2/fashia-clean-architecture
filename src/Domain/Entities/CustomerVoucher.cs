namespace Fashia.Domain.Entities;

public class CustomerVoucher : BaseAuditableEntity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public int VoucherId { get; private set; }
    public Voucher Voucher { get; private set; } = null!;
    public DateTime? RedeemedAt { get; private set; }

    public DateTime ValidFrom { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public bool IsRedeemed { get; private set; }
    public CustomerVoucherStatus Status { get; private set; }

    private CustomerVoucher()
    {
        // EF Core
    }

    public CustomerVoucher(int customerId, int voucherId, DateTime validFrom, DateTime validUntil)
    {
        SetCustomerId(customerId);
        SetVoucherId(voucherId);
        SetDateRange(validFrom, validUntil);
        IsRedeemed = false;
        Status = CustomerVoucherStatus.Active;
    }

    public void Redeem()
    {
        if (IsRedeemed)
            throw new InvalidOperationException("Voucher has already been redeemed.");

        if (DateTime.UtcNow < ValidFrom || DateTime.UtcNow > ValidUntil)
            throw new InvalidOperationException("Voucher is not valid at this time.");

        IsRedeemed = true;
        RedeemedAt = DateTime.UtcNow;
        Status = CustomerVoucherStatus.Inactive;
    }

    public void Revoke()
    {
        if (Status == CustomerVoucherStatus.Revoked)
            throw new InvalidOperationException("Voucher has already been revoked.");

        Status = CustomerVoucherStatus.Revoked;
    }

    public void SetDateRange(DateTime validFrom, DateTime validUntil)
    {
        if (validFrom >= validUntil)
            throw new ArgumentException("Valid from date must be earlier than valid until date.");

        ValidFrom = validFrom;
        ValidUntil = validUntil;
    }

    private void SetCustomerId(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer ID must be a positive integer.");

        CustomerId = customerId;
    }

    private void SetVoucherId(int voucherId)
    {
        if (voucherId <= 0)
            throw new ArgumentException("Voucher ID must be a positive integer.");

        VoucherId = voucherId;
    }
}
