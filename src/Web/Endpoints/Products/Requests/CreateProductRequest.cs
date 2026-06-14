namespace Fashia.Web.Endpoints.Products.Requests;

public sealed record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public List<int>? UploadedImageIds { get; init; } = [];

    public int CategoryId { get; init; }

    public int BrandId { get; init; }

    public List<CreateProductVariantRequest?>? Variants { get; init; } = [];
}
