namespace Fashia.Application.Vouchers.Queries;

public sealed record VoucherDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string DiscountType { get; init; } = string.Empty;
    public int DiscountAmount { get; init; }
    public int MinOrderAmount { get; init; }
    public int MaxDiscountAmount { get; init; }
    public int UsageLimit { get; init; }
    public int UsedCount { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }
    public string VoucherType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Display { get; init; } = string.Empty;
    public int QuantityPerUser { get; init; }
}
