using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Gender).HasMaxLength(10);
        builder.Property(u => u.TwoFASecret).HasMaxLength(255);
        builder.Property(u => u.Provider).HasMaxLength(50);
        builder.Property(u => u.ProviderId).HasMaxLength(200);

        // Unique external login (Provider, ProviderId) — chỉ áp khi cả 2 not null
        builder.HasIndex(u => new { u.Provider, u.ProviderId })
            .IsUnique()
            .HasFilter("[Provider] IS NOT NULL AND [ProviderId] IS NOT NULL");

        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);   // không cho xóa Role khi còn User

        // Soft-delete: mọi query mặc định bỏ qua bản ghi đã xóa
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
