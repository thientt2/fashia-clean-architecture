using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.CustomerId).IsRequired(false);
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PaymentMethod).HasConversion<int>().IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder
            .Property(x => x.CustomerEmail)
            .HasConversion(email => email.Value, value => EmailVO.Create(value))
            .HasMaxLength(200);
        builder
            .Property(x => x.CustomerPhone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(
            x => x.ShippingAddress,
            address =>
            {
                address.Property(x => x.Line1).HasMaxLength(200).IsRequired();
                address.Property(x => x.Ward).HasMaxLength(100).IsRequired();
                address.Property(x => x.District).HasMaxLength(100).IsRequired();
                address.Property(x => x.Province).HasMaxLength(100).IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.SubTotalAmount,
            money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.DiscountAmount,
            money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.TotalAmount,
            money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.ShippingFee,
            money =>
            {
                money.Property(m => m.Amount).HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasMaxLength(3).IsRequired();
            }
        );

        builder
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
