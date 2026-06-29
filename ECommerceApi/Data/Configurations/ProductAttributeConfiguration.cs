using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("Attributes");   // tên bảng theo schema
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();

        builder.Property(a => a.Type)
            .HasConversion<string>()     // lưu enum dạng "Select"/"Number"/"Text" cho dễ đọc
            .HasMaxLength(20);
    }
}
