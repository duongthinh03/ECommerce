using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class EmailOtpConfiguration : IEntityTypeConfiguration<EmailOtp>
{
    public void Configure(EntityTypeBuilder<EmailOtp> builder)
    {
        builder.ToTable("EmailOtps");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Email).HasMaxLength(256).IsRequired();
        builder.Property(o => o.OtpCode).HasMaxLength(10).IsRequired();
        builder.Property(o => o.Purpose).HasMaxLength(20).IsRequired();
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(o => new { o.Email, o.Purpose });
    }
}
