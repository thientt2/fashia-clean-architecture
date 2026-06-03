using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class CustomerVoucherConfiguration : IEntityTypeConfiguration<CustomerVoucher>
{
    public void Configure(EntityTypeBuilder<CustomerVoucher> builder)
    {
        builder.Property(x => x.ValidFrom).IsRequired();
        builder.Property(x => x.ValidUntil).IsRequired();
        builder.Property(x => x.IsRedeemed).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Voucher)
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
