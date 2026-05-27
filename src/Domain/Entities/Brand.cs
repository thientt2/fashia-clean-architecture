using Fashia.Domain.Common;

namespace Fashia.Domain.Entities;

public class Brand : BaseAuditableEntity
{
    private readonly List<Product> _products = new();
    private Brand()
    {
        // EF Core
    }

    public Brand(string name, string? description = null, string? imageUrl = null)
    {
        SetName(name);
        SetDescription(description);
        SetImageUrl(imageUrl);
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ImageUrl { get; private set; }

    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    public void Rename(string name)
    {
        SetName(name);
    }

    public void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public void UpdateImageUrl(string? imageUrl)
    {
        SetImageUrl(imageUrl);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Brand name must not be empty.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Brand name must not exceed 100 characters.", nameof(name));

        Name = name;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
            throw new ArgumentException("Brand description must not exceed 500 characters.", nameof(description));

        Description = description;
    }

    private void SetImageUrl(string? imageUrl)
    {
        if (imageUrl != null && imageUrl.Length > 250)
            throw new ArgumentException("Brand image URL must not exceed 250 characters.", nameof(imageUrl));

        ImageUrl = imageUrl;
    }
    public void AddProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (product.BrandId != Id)
            throw new InvalidOperationException("Product does not belong to this brand.");

        _products.Add(product);
    } 
}