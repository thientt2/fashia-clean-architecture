using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class OrderVoucherConfiguration : IEntityTypeConfiguration<OrderVoucher>
{
    public void Configure(EntityTypeBuilder<OrderVoucher> builder)
    {
        builder.Property(x => x.VoucherId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.VoucherCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DiscountType).HasConversion<int>().IsRequired();
        builder.Property(x => x.DiscountValue).IsRequired();

        builder.OwnsOne(
            x => x.DiscountAmount,
            discountAmount =>
            {
                discountAmount
                    .Property(d => d.Amount)
                    .HasColumnName("DiscountAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                discountAmount
                    .Property(d => d.Currency)
                    .HasColumnName("DiscountCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );
        builder.Property(x => x.AppliedAt).IsRequired();

        builder
            .HasOne<Order>()
            .WithOne(x => x.OrderVoucher)
            .HasForeignKey<OrderVoucher>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Voucher>()
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
