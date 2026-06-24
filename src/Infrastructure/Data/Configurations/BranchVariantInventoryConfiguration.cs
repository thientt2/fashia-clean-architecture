using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class BranchVariantInventoryConfiguration : IEntityTypeConfiguration<BranchVariantInventory>
{
    public void Configure(EntityTypeBuilder<BranchVariantInventory> builder)
    {
        builder.HasKey(x => new { x.BranchId, x.ProductVariantId });

        builder.Property(x => x.StockQuantity).IsRequired();
        builder.Property(x => x.ReservedQuantity).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();
        builder.Ignore(x => x.AvailableQuantity);

        builder.HasIndex(x => new { x.BranchId, x.ProductVariantId }).IsUnique();

        builder
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
