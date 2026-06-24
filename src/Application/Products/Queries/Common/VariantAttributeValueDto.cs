namespace Fashia.Application.Products.Queries.Common;

public sealed class VariantAttributeValueDto
{
    public int AttributeId { get; set; }

    public string AttributeName { get; set; } = string.Empty;

    public int AttributeValueId { get; set; }

    public string Value { get; set; } = string.Empty;
}
