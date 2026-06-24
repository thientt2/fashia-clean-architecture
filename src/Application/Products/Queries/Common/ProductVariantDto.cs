namespace Fashia.Application.Products.Queries.Common;

public sealed class ProductVariantDto
{
    public int Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public long OriginalPrice { get; set; }

    public decimal DiscountPercentage { get; set; }

    public long FinalPrice { get; set; }

    public int Quantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;

    public List<int> AttributeValueIds { get; set; } = [];

    public List<VariantAttributeValueDto> AttributeValues { get; set; } = [];
}
