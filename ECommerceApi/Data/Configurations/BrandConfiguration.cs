using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(150).IsRequired();
        builder.Property(b => b.Slug).HasMaxLength(170).IsRequired();
        builder.HasIndex(b => b.Slug).IsUnique();
        builder.Property(b => b.Description).HasMaxLength(1000);

        builder.Property(b => b.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasQueryFilter(b => b.DeletedAt == null);
    }
}
