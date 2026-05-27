namespace Fashia.Domain.Entities;

public class ProductAttribute : BaseAuditableEntity
{
    private ProductAttribute() { }

    public ProductAttribute(string name)
    {
        SetName(name);
    }

    public string Name { get; private set; } = string.Empty;

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name is required.", nameof(name));

        Name = name.Trim();
    }
}