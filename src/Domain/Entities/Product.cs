namespace Fashia.Domain.Entities;

public class Product : BaseAuditableEntity
{
    private readonly List<ProductVariant> _variants = new();

    private Product()
    {
        // EF Core
    }

    public Product(
        string name,
        int categoryId,
        int brandId,
        string? description = null,
        string? imageUrl = null)
    {
        SetName(name);
        SetCategory(categoryId);
        SetBrand(brandId);
        SetDescription(description);
        SetImageUrl(imageUrl);

        Status = ProductStatus.Active;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? ImageUrl { get; private set; }

    public int CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public int BrandId { get; private set; }

    public Brand Brand { get; private set; } = null!;

    public ProductStatus Status { get; private set; } = ProductStatus.Inactive;

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

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

    public void ChangeCategory(int categoryId)
    {
        SetCategory(categoryId);
    }

    public void ChangeBrand(int brandId)
    {
        SetBrand(brandId);
    }

    public void Activate()
    {
        if (Status == ProductStatus.Active)
            return;

        Status = ProductStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == ProductStatus.Inactive)
            return;

        Status = ProductStatus.Inactive;
    }

    public void AddVariant(
        decimal originalPrice,
        int stockQuantity,
        IEnumerable<int> attributeValueIds)
    {
        var variant = new ProductVariant(
            originalPrice,
            stockQuantity,
            attributeValueIds);

        _variants.Add(variant);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        var value = name.Trim();

        if (value.Length > 200)
            throw new ArgumentException("Product name must not exceed 200 characters.", nameof(name));

        Name = value;
    }

    private void SetDescription(string? description)
    {
        var value = description?.Trim() ?? string.Empty;

        if (value.Length > 2000)
            throw new ArgumentException("Product description must not exceed 2000 characters.", nameof(description));

        Description = value;
    }

    private void SetImageUrl(string? imageUrl)
    {
        var value = imageUrl?.Trim();

        if (!string.IsNullOrEmpty(value) && value.Length > 500)
            throw new ArgumentException("Product image URL must not exceed 500 characters.", nameof(imageUrl));

        ImageUrl = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private void SetCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Category id is required.", nameof(categoryId));

        CategoryId = categoryId;
    }

    private void SetBrand(int brandId)
    {
        if (brandId <= 0)
            throw new ArgumentException("Brand id is required.", nameof(brandId));

        BrandId = brandId;
    }
}