using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.Property(x => x.OriginalPrice).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.DiscountPercentage).HasPrecision(5, 2).IsRequired();

        builder.Ignore(x => x.SellingPrice);

        builder
            .HasMany(x => x.AttributeValues)
            .WithOne(x => x.ProductVariant)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
