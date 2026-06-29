using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(170).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(1000);

        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);   // không cho xóa cha khi còn con

        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}
