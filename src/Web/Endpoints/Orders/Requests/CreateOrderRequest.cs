namespace Fashia.Web.Endpoints.Orders.Requests;

public sealed record CreateOrderRequest
{
    public int CustomerId { get; init; }

    public int BranchId { get; init; }

    public string? VoucherCode { get; init; }

    public List<CreateOrderItemRequest> Items { get; init; } = [];
}

public sealed record CreateOrderItemRequest
{
    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }
}

public sealed record CheckoutOrderRequest
{
    public int BranchId { get; init; }

    public string? VoucherCode { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string? CustomerEmail { get; init; }

    public string CustomerPhone { get; init; } = string.Empty;

    public string ShippingAddress { get; init; } = string.Empty;
}
