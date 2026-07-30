using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Addresses");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.FullName).HasMaxLength(100).IsRequired();
            builder.Property(a => a.Phone).HasMaxLength(20).IsRequired();
            builder.Property(a => a.Province).HasMaxLength(100).IsRequired();
            builder.Property(a => a.District).HasMaxLength(100).IsRequired();
            builder.Property(a => a.Ward).HasMaxLength(100).IsRequired();
            builder.Property(a => a.AddressLine).HasMaxLength(255).IsRequired();

            builder.Property(a => a.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(a => a.UserId);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);   // xóa user → xóa địa chỉ

            builder.HasQueryFilter(a => a.DeletedAt == null);   // ẩn bản ghi đã soft-delete
        }
    }
}
