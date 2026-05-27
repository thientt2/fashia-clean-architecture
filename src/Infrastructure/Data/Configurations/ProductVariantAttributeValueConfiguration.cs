using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class ProductVariantAttributeValueConfiguration
    : IEntityTypeConfiguration<ProductVariantAttributeValue>
{
    public void Configure(
        EntityTypeBuilder<ProductVariantAttributeValue> builder)
    {
        builder.HasKey(x => new
        {
            x.ProductVariantId,
            x.AttributeValueId
        });

        builder.HasOne(x => x.ProductVariant)
            .WithMany(x => x.AttributeValues)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AttributeValue)
            .WithMany()
            .HasForeignKey(x => x.AttributeValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}