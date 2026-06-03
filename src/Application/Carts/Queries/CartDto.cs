namespace Fashia.Application.Carts.Queries;

public sealed record CartDto
{
    public int Id { get; init; }
    public int? CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyCollection<CartItemDto> Items { get; init; } = [];
}
