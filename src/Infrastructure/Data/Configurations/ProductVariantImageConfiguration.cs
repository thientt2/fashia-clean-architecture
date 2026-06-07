using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class ProductVariantImageConfiguration : IEntityTypeConfiguration<ProductVariantImage>
{
    public void Configure(EntityTypeBuilder<ProductVariantImage> builder)
    {
        builder
            .HasOne(x => x.ProductVariant)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.UploadedFile)
            .WithMany()
            .HasForeignKey(x => x.UploadedFileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IsMain).IsRequired();

        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.HasIndex(x => new { x.ProductVariantId, x.UploadedFileId }).IsUnique();

        builder.HasIndex(x => new { x.ProductVariantId, x.IsMain });
    }
}
