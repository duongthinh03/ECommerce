using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(280).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Description).HasColumnType("nvarchar(max)");

        builder.Property(p => p.DisplayPrice).HasPrecision(18, 2);
        builder.Property(p => p.Thumbnail).HasMaxLength(500);

        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);    // xóa brand → product.BrandId = null

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
