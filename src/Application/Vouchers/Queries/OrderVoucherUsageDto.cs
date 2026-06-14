namespace Fashia.Application.Vouchers.Queries;

public sealed record OrderVoucherUsageDto
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public int? CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int VoucherId { get; init; }
    public string VoucherCode { get; init; } = string.Empty;
    public long OrderSubTotalAmount { get; init; }
    public long DiscountAmount { get; init; }
    public long OrderTotalAmount { get; init; }
    public DateTime AppliedAt { get; init; }
}
