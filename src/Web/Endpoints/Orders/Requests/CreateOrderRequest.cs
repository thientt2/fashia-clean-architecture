using Fashia.Domain.Enums;

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
    public List<int> CartItemIds { get; init; } = [];
    public int ShippingAddressId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Note { get; init; }
}
