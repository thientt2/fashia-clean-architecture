namespace Fashia.Domain.Entities;

public class ProductVariant : BaseAuditableEntity
{
    private readonly List<ProductVariantAttributeValue> _attributeValues = new();
    public int ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public Money OriginalPrice { get; private set; } = null!;

    public Percentage DiscountPercentage { get; private set; } = null!;

    public Money SellingPrice => OriginalPrice.Multiply(1 - DiscountPercentage.Value / 100);

    public IReadOnlyCollection<ProductVariantAttributeValue> AttributeValues =>
        _attributeValues.AsReadOnly();

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
