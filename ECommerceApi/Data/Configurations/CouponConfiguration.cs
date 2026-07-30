using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();

        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.DiscountValue).HasPrecision(18, 2);
        builder.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);
        builder.Property(c => c.MinOrderAmount).HasPrecision(18, 2);

        builder.Property(c => c.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}
