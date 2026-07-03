using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Models
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistories");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(h => h.Note).HasMaxLength(500);
            builder.Property(h => h.ChangedBy).HasMaxLength(100);
            builder.Property(h => h.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(h => h.Order)
                .WithMany(o => o.StatusHistories)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
