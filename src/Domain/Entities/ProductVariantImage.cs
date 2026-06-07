namespace Fashia.Domain.Entities;

public class ProductVariantImage : BaseAuditableEntity
{
    public int ProductVariantId { get; private set; }

    public int UploadedFileId { get; private set; }

    public bool IsMain { get; private set; }

    public int DisplayOrder { get; private set; }

    public ProductVariant ProductVariant { get; private set; } = null!;
    public UploadedFile UploadedFile { get; private set; } = null!;

    private ProductVariantImage() { }

    public ProductVariantImage(int uploadedFileId, bool isMain, int displayOrder)
    {
        UploadedFileId = uploadedFileId;
        IsMain = isMain;
        DisplayOrder = displayOrder;
    }
}
