// Application/Products/Queries/Common/ImageDto.cs
namespace Fashia.Application.Products.Queries.Common;

public sealed class ImageDto
{
    public int UploadedFileId { get; init; }
    public string Url { get; init; } = string.Empty;
    public bool IsMain { get; init; }
    public int DisplayOrder { get; init; }
}
