using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.VariantName).HasMaxLength(200);

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
