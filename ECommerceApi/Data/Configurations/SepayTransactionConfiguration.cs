using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations;

public class SepayTransactionConfiguration : IEntityTypeConfiguration<SepayTransaction>
{
    public void Configure(EntityTypeBuilder<SepayTransaction> builder)
    {
        builder.ToTable("SepayTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Gateway).HasMaxLength(100);
        builder.Property(t => t.TransferType).HasMaxLength(10);
        builder.Property(t => t.Content).HasMaxLength(500);
        builder.Property(t => t.ReferenceCode).HasMaxLength(100);
        builder.Property(t => t.TransferAmount).HasPrecision(18, 2);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(t => t.SepayId).IsUnique();   // chống xử lý trùng webhook
    }
}
