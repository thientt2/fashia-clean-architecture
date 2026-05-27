using Fashia.Domain.Entities;

namespace Fashia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductAttribute> Attributes { get; }
    DbSet<ProductAttributeValue> AttributeValues { get; }
    DbSet<ProductVariantAttributeValue> VariantAttributeValues { get; }

    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
