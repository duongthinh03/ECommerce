using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.ToTable("CategoryAttributes");
        builder.HasKey(ca => ca.Id);

        // 1 cặp (Category, Attribute) chỉ khai báo 1 lần
        builder.HasIndex(ca => new { ca.CategoryId, ca.AttributeId }).IsUnique();

        builder.HasOne(ca => ca.Category)
            .WithMany()
            .HasForeignKey(ca => ca.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.Attribute)
            .WithMany(a => a.CategoryAttributes)
            .HasForeignKey(ca => ca.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);   // Restrict: tránh multiple cascade paths
    }
}
