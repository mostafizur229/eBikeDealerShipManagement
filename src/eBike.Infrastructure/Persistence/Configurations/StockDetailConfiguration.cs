using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class StockDetailConfiguration : IEntityTypeConfiguration<StockDetail>
{
    public void Configure(EntityTypeBuilder<StockDetail> builder)
    {
        builder.ToTable("StockDetails");
        builder.HasKey(e => e.SDetailID);

        builder.Property(e => e.StockCode).HasMaxLength(250).IsRequired();
        builder.Property(e => e.IMENO).IsRequired();
        builder.Property(e => e.EngineNo).IsRequired();
        builder.Property(e => e.KeyNo).IsRequired();
        builder.Property(e => e.PRate).HasPrecision(18, 2);
        builder.Property(e => e.SRate).HasPrecision(18, 2);
        builder.Property(e => e.CreditSRate).HasPrecision(18, 2);
        builder.Property(e => e.CRSalesRate12Month).HasPrecision(18, 2);
        builder.Property(e => e.CRSalesRate3Month).HasPrecision(18, 2);
        builder.Property(e => e.Quantity).HasPrecision(18, 2);

        builder.HasOne(e => e.Stock)
            .WithMany(s => s.StockDetails)
            .HasForeignKey(e => e.StockID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Color)
            .WithMany()
            .HasForeignKey(e => e.ColorID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.POrderDetail)
            .WithMany()
            .HasForeignKey(e => e.POrderDetailID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Godown)
            .WithMany()
            .HasForeignKey(e => e.GodownID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
