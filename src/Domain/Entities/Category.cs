using Fashia.Domain.Common;

namespace Fashia.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 1000;
    public const int MaxSlugLength = 250;
    public const int MaxImageUrlLength = 500;

    private readonly List<Category> _children = new();
    private Category()
    {
        // EF Core
    }

    public Category(
        string name,
        string? description = null,
        string? imageUrl = null,
        int? parentId = null)
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

        if (value.Length > MaxNameLength)
            throw new ArgumentException($"Category name must not exceed {MaxNameLength} characters.", nameof(name));

        Name = value;
        Slug = SlugGenerator.Generate(value);

        if (Slug.Length > MaxSlugLength)
            throw new ArgumentException($"Category slug must not exceed {MaxSlugLength} characters.", nameof(name));
    }

    private void SetDescription(string? description)
    {
        var value = description?.Trim() ?? string.Empty;

        if (value.Length > MaxDescriptionLength)
            throw new ArgumentException($"Category description must not exceed {MaxDescriptionLength} characters.", nameof(description));

        Description = value;
    }

    private void SetImageUrl(string? imageUrl)
    {
        var value = imageUrl?.Trim();

        if (!string.IsNullOrEmpty(value) && value.Length > MaxImageUrlLength)
            throw new ArgumentException($"Category image URL must not exceed {MaxImageUrlLength} characters.", nameof(imageUrl));

        ImageUrl = string.IsNullOrWhiteSpace(value) ? null : value;
    }    
}