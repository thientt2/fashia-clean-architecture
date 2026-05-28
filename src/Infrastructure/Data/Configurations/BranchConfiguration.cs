using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();

        builder.Property(x => x.Address).HasMaxLength(500).IsRequired();

        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.Phone).HasMaxLength(12).IsRequired();

        builder.Property(x => x.Latitude).HasColumnType("decimal(9,6)").IsRequired();

        builder.Property(x => x.Longitude).HasColumnType("decimal(9,6)").IsRequired();

        builder.Property(x => x.IsMain).IsRequired();
    }
}
