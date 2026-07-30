using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Price).HasPrecision(18, 2);
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            // 1 variant chỉ 1 dòng trong 1 giỏ (cộng dồn, không trùng)
            builder.HasIndex(i => new { i.CartId, i.VariantId }).IsUnique();

            builder.HasOne(i => i.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);    // xóa giỏ → xóa item

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Variant)
                .WithMany()
                .HasForeignKey(i => i.VariantId)
                .OnDelete(DeleteBehavior.Restrict);   // Restrict: tránh multiple cascade paths
        }
    }
}