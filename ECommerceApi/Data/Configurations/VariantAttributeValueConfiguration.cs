using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("VariantAttributeValues");
        builder.HasKey(x => x.Id);

        // 1 variant chỉ có 1 value cho mỗi attribute (không vừa Size 42 vừa Size 43)
        builder.HasIndex(x => new { x.VariantId, x.AttributeId }).IsUnique();

        builder.HasOne(x => x.Variant)
            .WithMany()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);    // xóa variant → xóa tổ hợp option theo

        builder.HasOne(x => x.Attribute)
            .WithMany(a => a.VariantAttributeValues)
            .HasForeignKey(x => x.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);   // Restrict: tránh multiple cascade paths

        builder.HasOne(x => x.AttributeValue)
            .WithMany(v => v.VariantAttributeValues)
            .HasForeignKey(x => x.AttributeValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
