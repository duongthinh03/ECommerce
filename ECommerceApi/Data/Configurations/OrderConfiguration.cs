using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderCode).HasMaxLength(30).IsRequired();
            builder.HasIndex(o => o.OrderCode).IsUnique();

            builder.Property(o => o.ShipRecipient).HasMaxLength(100).IsRequired();
            builder.Property(o => o.ShipPhone).HasMaxLength(20).IsRequired();
            builder.Property(o => o.ShipProvince).HasMaxLength(100).IsRequired();
            builder.Property(o => o.ShipDistrict).HasMaxLength(100).IsRequired();
            builder.Property(o => o.ShipWard).HasMaxLength(100).IsRequired();
            builder.Property(o => o.ShipAddressLine).HasMaxLength(255).IsRequired();

            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
            builder.Property(o => o.ShippingFee).HasPrecision(18, 2);
            builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
            builder.Property(o => o.FinalAmount).HasPrecision(18, 2);

            builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);
            builder.Property(o => o.PaymentMethod).HasMaxLength(30);
            builder.Property(o => o.Note).HasMaxLength(500);

            builder.Property(o => o.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(o => o.DeletedAt == null);
        }
    }
}
