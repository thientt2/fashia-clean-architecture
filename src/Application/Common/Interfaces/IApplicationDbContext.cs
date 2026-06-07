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
    DbSet<Branch> Branches { get; }
    DbSet<BranchVariantInventory> BranchVariantInventories { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderVoucher> OrderVouchers { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<CustomerVoucher> CustomerVouchers { get; }
    DbSet<UploadedFile> UploadedFiles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
