using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Models
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductName).HasMaxLength(250).IsRequired();
            builder.Property(i => i.Sku).HasMaxLength(100).IsRequired();
            builder.Property(i => i.ImageUrl).HasMaxLength(500);

            builder.Property(i => i.Price).HasPrecision(18, 2);
            builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
            builder.Property(i => i.FinalPrice).HasPrecision(18, 2);

            builder.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
