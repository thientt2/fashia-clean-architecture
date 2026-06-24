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
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderVoucher> OrderVouchers => Set<OrderVoucher>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<CustomerVoucher> CustomerVouchers => Set<CustomerVoucher>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariantImage> ProductVariantImages => Set<ProductVariantImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<bool> TryInsertIdempotencyKeyAsync(
        IdempotencyKey idempotencyKey,
        CancellationToken cancellationToken
    )
    {
        var affectedRows = await Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "IdempotencyKeys"
                ("Key", "CustomerId", "RequestHash", "CreatedAt", "ExpiresAt")
            VALUES
                ({idempotencyKey.Key}, {idempotencyKey.CustomerId}, {idempotencyKey.RequestHash},
                 {idempotencyKey.CreatedAt}, {idempotencyKey.ExpiresAt})
            ON CONFLICT ("Key") DO NOTHING
            """,
            cancellationToken
        );

        return affectedRows == 1;
    }

    public async Task CompleteIdempotencyKeyAsync(
        string key,
        int orderId,
        int responseStatusCode,
        string responseBody,
        CancellationToken cancellationToken
    )
    {
        var affectedRows = await Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE "IdempotencyKeys"
            SET "OrderId" = {orderId},
                "ResponseStatusCode" = {responseStatusCode},
                "ResponseBody" = CAST({responseBody} AS jsonb)
            WHERE "Key" = {key} AND "OrderId" IS NULL
            """,
            cancellationToken
        );

        if (affectedRows != 1)
            throw new DbUpdateConcurrencyException("Idempotency record could not be completed.");
    }

    public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(
        Func<Task<TResponse>> operation,
        CancellationToken cancellationToken
    )
    {
        var strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await BeginTransactionAsync(cancellationToken);

            var response = await operation();

            await transaction.CommitAsync(cancellationToken);

            return response;
        });
    }

    public async Task<IApplicationDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken
    )
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken);

        return new ApplicationDbContextTransaction(transaction);
    }
}
