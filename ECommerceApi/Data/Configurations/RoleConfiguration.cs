using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();

        builder.Property(r => r.Description).HasMaxLength(200);

        // Seed 4 vai trò cố định (Id giữ nguyên để code tham chiếu được)
        builder.HasData(
            new Role { Id = 1, Name = "Admin", Description = "Quản trị tối cao" },
            new Role { Id = 2, Name = "Manager", Description = "Quản trị / quản lý" },
            new Role { Id = 3, Name = "Staff", Description = "Nhân viên" },
            new Role { Id = 4, Name = "Customer", Description = "Khách hàng" }
        );
    }
}
