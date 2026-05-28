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
