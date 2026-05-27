using Fashia.Application.Products.Commands.CreateProduct;

namespace Fashia.Web.Endpoints.Requests;

public class CreateProductRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int CategoryId { get; init; }

    public int BrandId { get; init; }

    public IFormFile? Image { get; init; }

    public string Variants { get; init; } = "[]";   
}

public class CreateProductVariantRequest
{
    public decimal OriginalPrice { get; init; }

    public int StockQuantity { get; init; }
    public List<int> AttributeValueIds { get; init; } = [];
}
