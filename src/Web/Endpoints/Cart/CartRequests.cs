namespace Fashia.Web.Endpoints.Requests;

public sealed record AddCartItemRequest
{
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
}

public sealed record UpdateCartItemQuantityRequest
{
    public int Quantity { get; init; }
}
