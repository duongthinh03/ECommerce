using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("AttributeValues");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Value).HasMaxLength(150).IsRequired();

        // Không trùng value trong cùng 1 attribute (vd Size không có hai "42")
        builder.HasIndex(v => new { v.AttributeId, v.Value }).IsUnique();

        builder.HasOne(v => v.Attribute)
            .WithMany(a => a.Values)
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);   // xóa attribute → xóa values theo
    }
}
