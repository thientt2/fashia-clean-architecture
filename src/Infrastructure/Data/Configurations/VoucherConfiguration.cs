using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.DiscountType).HasConversion<int>().IsRequired();
        builder.Property(x => x.DiscountAmount).IsRequired();
        builder.Property(x => x.MinOrderAmount).IsRequired();
        builder.Property(x => x.MaxDiscountAmount).IsRequired();
        builder.Property(x => x.UsageLimit).IsRequired();
        builder.Property(x => x.UsedCount).IsRequired();
        builder.Property(x => x.ValidFrom).IsRequired();
        builder.Property(x => x.ValidUntil).IsRequired();
        builder.Property(x => x.VoucherType).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Display).HasConversion<int>().IsRequired();
        builder.Property(x => x.QuantityPerUser).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
