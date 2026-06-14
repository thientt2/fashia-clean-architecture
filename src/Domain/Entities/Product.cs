namespace Fashia.Domain.Entities;

public class Product : BaseAuditableEntity
{
    private readonly List<ProductVariant> _variants = new();
    private readonly List<ProductImage> _images = new();

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public int BrandId { get; private set; }

    public Brand Brand { get; private set; } = null!;

    public ProductStatus Status { get; private set; }

    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private Product()
    {
        // EF Core
    }

    public Product(
        string name,
        int categoryId,
        int brandId,
        string description,
        List<int> uploadedImageIds
    )
    {
        SetName(name);
        SetCategory(categoryId);
        SetBrand(brandId);
        SetDescription(description);
        Status = ProductStatus.Active;

        if (uploadedImageIds is null || uploadedImageIds.Count == 0)
            throw new ArgumentException(
                "Product must have at least one image.",
                nameof(uploadedImageIds)
            );

        var order = 0;
        foreach (var imageId in uploadedImageIds)
        {
            AddImage(imageId, isMain: order == 0, displayOrder: order);
            order++;
        }
    }

    public void Rename(string name)
    {
        SetName(name);
    }

    public void AddImage(int uploadedFileId, bool isMain = false, int displayOrder = 0)
    {
        if (_images.Any(x => x.UploadedFileId == uploadedFileId))
            return;

        if (isMain && _images.Any(x => x.IsMain))
            throw new InvalidOperationException("Product already has a main image.");

        _images.Add(new ProductImage(uploadedFileId, isMain, displayOrder));
    }

    public void UpdateDescription(string description)
    {
        SetDescription(description);
    }

    public void RemoveImage(int uploadedFileId)
    {
        var image = _images.FirstOrDefault(x => x.UploadedFileId == uploadedFileId);

        if (image is null)
            return;

        _images.Remove(image);
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

    public ProductVariant AddVariant(Money originalPrice, IEnumerable<int> attributeValueIds)
    {
        var variant = ProductVariant.Create(originalPrice, attributeValueIds);
        _variants.Add(variant);
        return variant;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        var value = name.Trim();

        if (value.Length > 200)
            throw new ArgumentException(
                "Product name must not exceed 200 characters.",
                nameof(name)
            );

        Name = value;
    }

    private void SetDescription(string description)
    {
        var value = description?.Trim() ?? string.Empty;

        if (value.Length > 1000)
            throw new ArgumentException(
                "Product description must not exceed 1000 characters.",
                nameof(description)
            );

        Description = value;
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
