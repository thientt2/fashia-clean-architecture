namespace Fashia.Domain.Entities;
public class ProductVariantAttributeValue
{
    private ProductVariantAttributeValue() { }

    public ProductVariantAttributeValue(int attributeValueId)
    {
        if (attributeValueId <= 0)
            throw new ArgumentException("Attribute value id is required.");

        AttributeValueId = attributeValueId;
    }

    public int ProductVariantId { get; private set; }

    public ProductVariant ProductVariant { get; private set; } = null!;

    public int AttributeValueId { get; private set; }

    public ProductAttributeValue AttributeValue { get; private set; } = null!;
}