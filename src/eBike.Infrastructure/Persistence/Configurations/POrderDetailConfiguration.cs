using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class POrderDetailConfiguration : IEntityTypeConfiguration<POrderDetail>
{
    public void Configure(EntityTypeBuilder<POrderDetail> builder)
    {
        builder.ToTable("POrderDetails");
        builder.HasKey(e => e.POrderDetailID);

        builder.Property(e => e.Quantity).HasPrecision(18, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TAmount).HasPrecision(18, 2);
        builder.Property(e => e.PPDISPer).HasPrecision(18, 2);
        builder.Property(e => e.PPDISAmt).HasPrecision(18, 2);
        builder.Property(e => e.MRPRate).HasPrecision(18, 2);
        builder.Property(e => e.SalesRate).HasPrecision(18, 2);
        builder.Property(e => e.ExtraPPDISPer).HasPrecision(18, 2);
        builder.Property(e => e.ExtraPPDISAmt).HasPrecision(18, 2);
        builder.Property(e => e.PPOFFER).HasPrecision(18, 2);
        builder.Property(e => e.CreditSalesRate).HasPrecision(18, 2);
        builder.Property(e => e.CRSalesRate12Month).HasPrecision(18, 2);
        builder.Property(e => e.CRSalesRate3Month).HasPrecision(18, 2);
        builder.Property(e => e.Mushok).HasPrecision(18, 2);
        builder.Property(e => e.MRPWMushok).HasPrecision(18, 2);
        builder.Property(e => e.DPRate).HasPrecision(18, 2);
        builder.Property(e => e.RecQuantity).HasPrecision(18, 2);
        builder.Property(e => e.PWTaxAmt).HasPrecision(18, 2);

        builder.HasOne(e => e.POrder)
            .WithMany(p => p.POrderDetails)
            .HasForeignKey(e => e.POrderID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Color)
            .WithMany()
            .HasForeignKey(e => e.ColorID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Godown)
            .WithMany()
            .HasForeignKey(e => e.GodownID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.POProductDetails)
            .WithOne(e => e.POrderDetail)
            .HasForeignKey(e => e.POrderDetailID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
