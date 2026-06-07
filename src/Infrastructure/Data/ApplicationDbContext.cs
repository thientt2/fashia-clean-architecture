using System.Reflection;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<ProductAttribute> Attributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> AttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductVariantAttributeValue> VariantAttributeValues =>
        Set<ProductVariantAttributeValue>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<BranchVariantInventory> BranchVariantInventories => Set<BranchVariantInventory>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<CustomerVoucher> CustomerVouchers => Set<CustomerVoucher>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
