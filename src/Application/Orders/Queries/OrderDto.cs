namespace Fashia.Application.Orders.Queries;

public sealed record OrderDto
{
    public int Id { get; init; }

    public int? CustomerId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string? CustomerEmail { get; init; }

    public string CustomerPhone { get; init; } = string.Empty;

    public string ShippingAddress { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    public int? VoucherId { get; init; }

    public string? VoucherCode { get; init; }

    public decimal SubTotalAmount { get; init; }

    public decimal DiscountAmount { get; init; }

    public decimal TotalAmount { get; init; }

    public string Status { get; init; } = string.Empty;

    public IReadOnlyCollection<OrderItemDto> Items { get; init; } = [];

    public IReadOnlyCollection<OrderVoucherDto> Vouchers { get; init; } = [];
}

public sealed record OrderItemDto
{
    public int Id { get; init; }

    public int ProductVariantId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal UnitPrice { get; init; }

    public decimal LineTotal { get; init; }
}

public sealed record OrderVoucherDto
{
    public int Id { get; init; }

    public int VoucherId { get; init; }

    public string VoucherCode { get; init; } = string.Empty;

    public decimal DiscountAmount { get; init; }

    public DateTime AppliedAt { get; init; }
}
