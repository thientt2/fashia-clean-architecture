using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fashia.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(Category.MaxDescriptionLength)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(Category.MaxSlugLength)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(Category.MaxImageUrlLength);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}