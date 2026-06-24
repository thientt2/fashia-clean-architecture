namespace Fashia.Application.Products.Queries.GetProducts;

public sealed class ProductListItemDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string BrandName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public long MinOriginalPrice { get; init; }

    public long MaxOriginalPrice { get; init; }

    public decimal MaxDiscountPercentage { get; init; }

    public long MinFinalPrice { get; init; }

    public long MaxFinalPrice { get; init; }

    public bool HasPriceRange => MinFinalPrice != MaxFinalPrice;

    public bool HasDiscount => MaxDiscountPercentage > 0;

    public string? ThumbnailImageUrl { get; init; }

    public int VariantCount { get; init; }
}
