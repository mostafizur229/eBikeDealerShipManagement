using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CreditSaleDetailConfiguration : IEntityTypeConfiguration<CreditSaleDetail>
{
    public void Configure(EntityTypeBuilder<CreditSaleDetail> builder)
    {
        builder.ToTable("CreditSaleDetails");
        builder.HasKey(e => e.CreditSaleDetailsID);

        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.Quantity).HasPrecision(18, 2);
        builder.Property(e => e.UTAmount).HasPrecision(18, 2);
        builder.Property(e => e.MPRateTotal).HasPrecision(18, 2);
        builder.Property(e => e.MPRate).HasPrecision(18, 2);
        builder.Property(e => e.PPOffer).HasPrecision(18, 2);
        builder.Property(e => e.IntPercentage).HasPrecision(18, 2);
        builder.Property(e => e.IntTotalAmt).HasPrecision(18, 2);
        builder.Property(e => e.SRate).HasPrecision(18, 2);
        builder.Property(e => e.PPVat).HasPrecision(18, 2);
        builder.Property(e => e.RegAmt).HasPrecision(18, 2);
        builder.Property(e => e.OfferAmt).HasPrecision(18, 2);
        builder.Property(e => e.Mushok).HasPrecision(18, 2);
        builder.Property(e => e.SRateWMushok).HasPrecision(18, 2);
        builder.Property(e => e.InsuranceAmt).HasPrecision(18, 2);
        builder.Property(e => e.ExtInstAmt).HasPrecision(18, 2);

        builder.HasOne(e => e.CreditSale)
            .WithMany(s => s.CreditSaleDetails)
            .HasForeignKey(e => e.CreditSalesID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.StockDetail)
            .WithMany()
            .HasForeignKey(e => e.StockDetailID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
