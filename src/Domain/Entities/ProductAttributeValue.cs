namespace Fashia.Domain.Entities;

public class ProductAttributeValue : BaseAuditableEntity
{
    private ProductAttributeValue() { }

    public ProductAttributeValue(int attributeId, string value, string? hexValue = null)
    {
        if (attributeId <= 0)
            throw new ArgumentException("Attribute id is required.", nameof(attributeId));

        SetValue(value);
        SetHexValue(hexValue);

        AttributeId = attributeId;
    }

    public int AttributeId { get; private set; }

    public ProductAttribute Attribute { get; private set; } = null!;

    public string Value { get; private set; } = string.Empty;
    public string? HexValue { get; private set; }
    private void SetValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value is required.", nameof(value));

        Value = value.Trim();
    }

    private void SetHexValue(string? hexValue)
    {
        var value = hexValue?.Trim();

        if (!string.IsNullOrEmpty(value) && value.Length > 7)
            throw new ArgumentException("Hex value must not exceed 7 characters.", nameof(hexValue));

        HexValue = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}