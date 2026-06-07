namespace Fashia.Domain.Entities;

public class ProductVariant : BaseAuditableEntity
{
    private readonly List<ProductVariantAttributeValue> _attributeValues = new();
    private readonly List<ProductVariantImage> _images = new();
    public int ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public Money OriginalPrice { get; private set; } = null!;

    public Percentage DiscountPercentage { get; private set; } = null!;

    public Money SellingPrice => OriginalPrice.Multiply(1 - DiscountPercentage.ToDecimal());

    public IReadOnlyCollection<ProductVariantAttributeValue> AttributeValues =>
        _attributeValues.AsReadOnly();
    public IReadOnlyCollection<ProductVariantImage> Images => _images.AsReadOnly();

    private ProductVariant()
    {
        // EF Core
    }

    public ProductVariant(Money originalPrice, IEnumerable<int> attributeValueIds)
    {
        OriginalPrice = originalPrice;
        DiscountPercentage = Percentage.Zero;

        foreach (var attributeValueId in attributeValueIds.Distinct())
        {
            AddAttributeValue(attributeValueId);
        }
    }

    public void ChangePrice(Money price)
    {
        OriginalPrice = price;
    }

    public void ReplaceAttributeValues(IEnumerable<int> attributeValueIds)
    {
        _attributeValues.Clear();

        foreach (var attributeValueId in attributeValueIds.Distinct())
        {
            AddAttributeValue(attributeValueId);
        }
    }

    public void AddImage(int uploadedFileId, int displayOrder = 0)
    {
        if (_images.Any(x => x.UploadedFileId == uploadedFileId))
            return;

        var isMain = !_images.Any(x => x.IsMain);

        _images.Add(new ProductVariantImage(uploadedFileId, isMain, displayOrder));
    }

    public void RemoveImage(int uploadedFileId)
    {
        var image = _images.FirstOrDefault(x => x.UploadedFileId == uploadedFileId);

        if (image is null)
            return;

        _images.Remove(image);
    }

    public void AddAttributeValue(int attributeValueId)
    {
        if (attributeValueId <= 0)
            throw new ArgumentException(
                "Attribute value id is required.",
                nameof(attributeValueId)
            );

        if (_attributeValues.Any(x => x.AttributeValueId == attributeValueId))
            return;

        _attributeValues.Add(new ProductVariantAttributeValue(attributeValueId));
    }
}
