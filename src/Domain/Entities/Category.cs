using Fashia.Domain.Common;

namespace Fashia.Domain.Entities;

public class Category : BaseAuditableEntity
{
    private readonly List<Category> _children = new();
    private readonly List<Product> _products = new();

    private Category()
    {
        // EF Core
    }

    public Category(
        string name,
        string? description = null,
        string? imageUrl = null,
        int? parentId = null
    )
    {
        SetName(name);
        SetDescription(description);
        SetImageUrl(imageUrl);
        MoveToParent(parentId);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string? ImageUrl { get; private set; }

    public bool IsActive { get; private set; } = true;

    public int? ParentId { get; private set; }

    public Category? Parent { get; private set; }

    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();
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

    public void MoveToParent(int? parentId)
    {
        if (parentId.HasValue && parentId.Value == Id)
            throw new InvalidOperationException("Category cannot be parent of itself.");

        ParentId = parentId;
    }

    public void AddChild(Category child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        if (child.Id == Id)
            throw new InvalidOperationException("Category cannot be child of itself.");

        if (_children.Any(c => c.Id == child.Id))
            return;

        child.MoveToParent(Id);
        _children.Add(child);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        AddDomainEvent(new CategoryActivatedEvent(this));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        AddDomainEvent(new CategoryDeactivatedEvent(this));
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        var value = name.Trim();

        if (value.Length > 200)
            throw new ArgumentException(
                "Category name must not exceed 200 characters.",
                nameof(name)
            );

        Name = value;
        Slug = SlugGenerator.Generate(value);

        if (Slug.Length > 250)
            throw new ArgumentException(
                "Category slug must not exceed 250 characters.",
                nameof(name)
            );
    }

    private void SetDescription(string? description)
    {
        var value = description?.Trim() ?? string.Empty;

        if (value.Length > 1000)
            throw new ArgumentException(
                "Category description must not exceed 1000 characters.",
                nameof(description)
            );

        Description = value;
    }

    private void SetImageUrl(string? imageUrl)
    {
        var value = imageUrl?.Trim();

        if (!string.IsNullOrEmpty(value) && value.Length > 500)
            throw new ArgumentException(
                "Category image URL must not exceed 500 characters.",
                nameof(imageUrl)
            );

        ImageUrl = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
