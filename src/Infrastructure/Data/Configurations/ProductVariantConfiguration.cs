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
                    .Property(p => p.BasisPoints)
                    .HasColumnName("DiscountPercentage")
                    .HasPrecision(5, 2);
            }
        );

        builder.OwnsOne(
            x => x.OriginalPrice,
            price =>
            {
                price
                    .Property(p => p.Amount)
                    .HasColumnName("OriginalPrice")
                    .HasColumnType("bigint");
            }
        );

        builder.Ignore(x => x.SellingPrice);

        builder
            .HasMany(x => x.AttributeValues)
            .WithOne(x => x.ProductVariant)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Images)
            .WithOne(x => x.ProductVariant)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
