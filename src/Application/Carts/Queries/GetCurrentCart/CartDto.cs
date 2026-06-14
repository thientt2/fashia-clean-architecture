namespace Fashia.Application.Carts.Queries.GetCurrentCart;

public sealed record CartDto
{
    public int? Id { get; init; }

    public IReadOnlyCollection<CartItemDto> Items { get; init; } = [];

    public decimal SubTotal => Items.Sum(x => x.LineTotal);
    public int TotalQuantity => Items.Sum(x => x.Quantity);
    public decimal TotalPrice => SubTotal;

    public static CartDto Empty()
    {
        return new CartDto { Id = null, Items = [] };
    }
}
