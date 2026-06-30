using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsages");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.DiscountAmount).HasPrecision(18, 2);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(u => u.Coupon)
            .WithMany(c => c.Usages)
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
