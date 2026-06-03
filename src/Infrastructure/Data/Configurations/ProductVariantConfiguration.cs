using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.OwnsOne(
            x => x.DiscountPercentage,
            discount =>
            {
                discount
                    .Property(p => p.Value)
                    .HasColumnName("DiscountPercentage")
                    .HasPrecision(5, 2);
            }
        );

        builder.OwnsOne(
            x => x.OriginalPrice,
            price =>
            {
                price.Property(p => p.Amount).HasColumnName("OriginalPrice").HasPrecision(18, 2);
            }
        );

        builder.Ignore(x => x.SellingPrice);

        builder
            .HasMany(x => x.AttributeValues)
            .WithOne(x => x.ProductVariant)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
