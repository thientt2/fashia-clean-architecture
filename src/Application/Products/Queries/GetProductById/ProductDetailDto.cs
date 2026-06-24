using Fashia.Application.Products.Queries.Common;

namespace Fashia.Application.Products.Queries.GetProductById;

public sealed class ProductDetailDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public List<ProductVariantOptionDto> VariantOptions { get; set; } = [];

    public List<ProductVariantDto> Variants { get; set; } = [];

    public List<ProductImageDto> Images { get; set; } = [];
}
