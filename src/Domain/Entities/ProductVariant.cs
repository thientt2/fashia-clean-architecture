namespace Fashia.Domain.Entities;

public class ProductVariant : BaseAuditableEntity
{
    private readonly List<ProductVariantAttributeValue> _attributeValues = new();

    private ProductVariant()
    {
        // EF Core
    }

    public ProductVariant(decimal originalPrice, IEnumerable<int> attributeValueIds)
    {
        SetOriginalPrice(originalPrice);

        foreach (var attributeValueId in attributeValueIds.Distinct())
        {
            AddAttributeValue(attributeValueId);
        }
    }

    public int ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public decimal OriginalPrice { get; private set; }

    public decimal DiscountPercentage { get; private set; }

    public decimal SellingPrice => OriginalPrice * (1 - DiscountPercentage / 100);

    public IReadOnlyCollection<ProductVariantAttributeValue> AttributeValues =>
        _attributeValues.AsReadOnly();

    public void ChangePrice(decimal price)
    {
        SetOriginalPrice(price);
    }

    public void ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException(
                "Discount percentage must be between 0 and 100.",
                nameof(discountPercentage)
            );

        DiscountPercentage = discountPercentage;
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

    private void SetOriginalPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        OriginalPrice = price;
    }
}
