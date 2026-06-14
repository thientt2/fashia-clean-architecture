namespace Fashia.Web.Endpoints.Products.Requests;

public sealed class CreateProductVariantRequest
{
    public long OriginalPrice { get; init; }
    public List<int>? UploadedImageIds { get; init; } = [];
    public List<int>? AttributeValueIds { get; init; } = [];
}
