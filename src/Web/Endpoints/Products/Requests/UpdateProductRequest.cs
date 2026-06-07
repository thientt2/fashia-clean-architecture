namespace Fashia.Web.Endpoints.Products.Requests;

public sealed class UpdateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int BrandId { get; init; }
    public List<int> NewUploadedImageIds { get; init; } = [];
}
