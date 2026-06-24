using Fashia.Domain.Enums;

namespace Fashia.Web.Endpoints.Orders.Requests;

public sealed record CreateOrderRequest
{
    public string? VoucherCode { get; init; }

    public List<CreateOrderItemRequest> Items { get; init; } = [];
}

public sealed record CreateOrderItemRequest
{
    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }
}

public sealed record PlaceOrderRequest
{
    public PlaceOrderShippingAddressRequest ShippingAddress { get; init; } = new();
    public string? VoucherCode { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Note { get; init; }
}

public sealed record PlaceOrderShippingAddressRequest
{
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string Line1 { get; init; } = string.Empty;
    public string Ward { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
}
