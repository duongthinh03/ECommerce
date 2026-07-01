using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku).HasMaxLength(100).IsRequired();
        builder.HasIndex(v => v.Sku).IsUnique();

        builder.Property(v => v.OptionName).HasMaxLength(100);

        builder.Property(v => v.Price).HasPrecision(18, 2);
        builder.Property(v => v.CompareAtPrice).HasPrecision(18, 2);
        builder.Property(v => v.Weight).HasPrecision(18, 3);
        builder.Property(v => v.ImageUrl).HasMaxLength(500);

        builder.Property(v => v.IsActive).HasDefaultValue(true);
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(v => v.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);    // xóa product → xóa variant theo

        builder.HasQueryFilter(v => v.DeletedAt == null);
    }
}
