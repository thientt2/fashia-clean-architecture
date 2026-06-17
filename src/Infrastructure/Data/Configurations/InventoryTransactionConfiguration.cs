using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();

        builder.Property(x => x.Quantity).IsRequired();

        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.PreviousStockQuantity);
        builder.Property(x => x.NewStockQuantity);
        builder.Property(x => x.PreviousReservedQuantity);
        builder.Property(x => x.NewReservedQuantity);
        builder.Property(x => x.SourceBranchId);
        builder.Property(x => x.DestinationBranchId);
        builder.Property(x => x.OrderId);
        builder.Property(x => x.TransferCorrelationId);

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
