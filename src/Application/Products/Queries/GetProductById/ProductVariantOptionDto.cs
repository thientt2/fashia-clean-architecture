namespace Fashia.Application.Products.Queries.GetProductById;

public sealed class ProductVariantOptionDto
{
    public int AttributeId { get; set; }

    public string AttributeName { get; set; } = string.Empty;

    public List<ProductVariantOptionValueDto> Values { get; set; } = [];
}
