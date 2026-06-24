namespace Fashia.Application.Products.Queries.GetProductById;

public sealed class ProductImageDto
{
    public int Id { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public bool IsMain { get; init; }
}
