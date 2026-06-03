using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.IsMain).IsRequired();

        builder.OwnsOne(
            x => x.Phone,
            phone =>
            {
                phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.Email,
            email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(200).IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.Location,
            location =>
            {
                location.Property(l => l.Latitude).HasColumnName("Latitude").IsRequired();
                location.Property(l => l.Longitude).HasColumnName("Longitude").IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.BranchAddress,
            address =>
            {
                address.Property(a => a.Line1).HasColumnName("Line1").IsRequired();
                address.Property(a => a.Ward).HasColumnName("Ward").IsRequired();
                address.Property(a => a.District).HasColumnName("District").IsRequired();
                address.Property(a => a.Province).HasColumnName("Province").IsRequired();
            }
        );
    }
}
