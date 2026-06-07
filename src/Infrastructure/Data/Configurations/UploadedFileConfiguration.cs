using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
{
    public void Configure(EntityTypeBuilder<UploadedFile> builder)
    {
        builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();

        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();

        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();

        builder.Property(x => x.PublicId).HasMaxLength(500).IsRequired();

        builder.Property(x => x.Folder).HasMaxLength(250).IsRequired();

        builder.HasIndex(x => x.PublicId).IsUnique();
    }
}
