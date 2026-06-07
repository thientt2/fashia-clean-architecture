namespace Fashia.Domain.Entities;

public class ProductImage : BaseAuditableEntity
{
    private ProductImage() { }

    public ProductImage(int uploadedFileId, bool isMain, int displayOrder)
    {
        UploadedFileId = uploadedFileId;
        IsMain = isMain;
        DisplayOrder = displayOrder;
    }

    public int ProductId { get; private set; }

    public int UploadedFileId { get; private set; }

    public bool IsMain { get; private set; }

    public int DisplayOrder { get; private set; }

    public Product Product { get; private set; } = null!;

    public UploadedFile UploadedFile { get; private set; } = null!;
}
