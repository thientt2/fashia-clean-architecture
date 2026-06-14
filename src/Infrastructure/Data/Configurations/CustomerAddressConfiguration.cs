using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder
            .Property(x => x.CustomerPhone)
            .HasConversion(x => x.Value, v => PhoneNumber.Create(v))
            .HasMaxLength(20)
            .IsRequired();

        builder
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address.Property(x => x.Line1).HasMaxLength(200).IsRequired();
                address.Property(x => x.Ward).HasMaxLength(100).IsRequired();
                address.Property(x => x.District).HasMaxLength(100).IsRequired();
                address.Property(x => x.Province).HasMaxLength(100).IsRequired();
            }
        );
    }
}
