using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProductVariantName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.VariantName).HasMaxLength(200);
        builder.Property(x => x.VariantAttributes).HasMaxLength(1000);
        builder.Property(x => x.Sku).HasMaxLength(100);

        builder.Ignore(x => x.LineTotal);

        builder.OwnsOne(
            x => x.UnitPrice,
            money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
            }
        );

        builder
            .HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
