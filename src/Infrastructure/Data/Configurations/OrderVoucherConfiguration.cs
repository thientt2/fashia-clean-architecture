using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class OrderVoucherConfiguration : IEntityTypeConfiguration<OrderVoucher>
{
    public void Configure(EntityTypeBuilder<OrderVoucher> builder)
    {
        builder.Property(x => x.VoucherCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.AppliedAt).IsRequired();

        builder.HasIndex(x => x.OrderId).IsUnique();
        builder.HasIndex(x => new { x.VoucherId, x.OrderId });

        builder
            .HasOne(x => x.Voucher)
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
