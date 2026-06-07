namespace Fashia.Application.Carts.Queries.GetCurrentCart;

public sealed record CartItemDto
{
    public int Id { get; init; }
    public int ProductVariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
