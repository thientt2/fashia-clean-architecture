using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using Fashia.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Points).IsRequired();
        builder.Property(x => x.Tier).HasConversion<int>().IsRequired();

        builder
            .Property(x => x.CustomerEmail)
            .HasConversion(email => email.Value, value => EmailVO.Create(value))
            .HasMaxLength(200)
            .IsRequired();
        builder
            .Property(x => x.CustomerPhone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
            .HasMaxLength(20)
            .IsRequired();

        builder
            .HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Customer>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
