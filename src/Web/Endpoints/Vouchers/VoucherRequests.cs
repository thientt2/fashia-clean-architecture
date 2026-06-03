using Fashia.Domain.Enums;

namespace Fashia.Web.Endpoints.Requests;

public sealed record CreateVoucherRequest
{
    public string Code { get; init; } = string.Empty;
    public DiscountType DiscountType { get; init; } = DiscountType.FixedAmount;
    public int DiscountAmount { get; init; }
    public int MinOrderAmount { get; init; }
    public int MaxDiscountAmount { get; init; }
    public int UsageLimit { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }
    public VoucherType VoucherType { get; init; } = VoucherType.All;
    public Display Display { get; init; } = Display.Public;
    public int QuantityPerUser { get; init; } = 1;
}

public sealed record UpdateVoucherRequest
{
    public string Code { get; init; } = string.Empty;
    public DiscountType DiscountType { get; init; } = DiscountType.FixedAmount;
    public int DiscountAmount { get; init; }
    public int MinOrderAmount { get; init; }
    public int MaxDiscountAmount { get; init; }
    public int UsageLimit { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }
    public VoucherType VoucherType { get; init; } = VoucherType.All;
    public Display Display { get; init; } = Display.Public;
    public int QuantityPerUser { get; init; } = 1;
}
