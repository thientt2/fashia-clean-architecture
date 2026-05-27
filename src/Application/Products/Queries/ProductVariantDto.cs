namespace Fashia.Application.Products.Queries;
public sealed class ProductVariantDto
{
    public int Id { get; init; }

    public decimal OriginalPrice { get; init; }

    public decimal DiscountPercentage { get; init; }

    public int StockQuantity { get; init; }

    public decimal SellingPrice { get; init; }

    public IReadOnlyCollection<ProductVariantAttributeValueDto> AttributeValues { get; init; } = [];
}
