namespace Fashia.Application.Carts.Queries.GetCurrentCart;

public sealed record CartDto
{
    public int? Id { get; init; }

    public IReadOnlyCollection<CartItemDto> Items { get; init; } = [];

    public decimal SubTotal => Items.Sum(x => x.LineTotal);

    public static CartDto Empty()
    {
        return new CartDto { Id = null, Items = [] };
    }
}
