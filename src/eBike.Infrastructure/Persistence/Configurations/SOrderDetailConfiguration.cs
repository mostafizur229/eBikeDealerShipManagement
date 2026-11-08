using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class SOrderDetailConfiguration : IEntityTypeConfiguration<SOrderDetail>
{
    public void Configure(EntityTypeBuilder<SOrderDetail> builder)
    {
        builder.ToTable("SOrderDetails");
        builder.HasKey(e => e.SOrderDetailID);

        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.PPDPercentage).HasPrecision(18, 2);
        builder.Property(e => e.PPDAmount).HasPrecision(18, 2);
        builder.Property(e => e.Quantity).HasPrecision(18, 2);
        builder.Property(e => e.UTAmount).HasPrecision(18, 2);
        builder.Property(e => e.MPRate).HasPrecision(18, 2);
        builder.Property(e => e.PPOffer).HasPrecision(18, 2);
        builder.Property(e => e.SRate).HasPrecision(18, 2);
        builder.Property(e => e.PRate).HasPrecision(18, 2);
        builder.Property(e => e.RepUnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.RQuantity).HasPrecision(18, 2);
        builder.Property(e => e.PPVat).HasPrecision(18, 2);
        builder.Property(e => e.OfferAmt).HasPrecision(18, 2);
        builder.Property(e => e.RegAmount).HasPrecision(18, 2);
        builder.Property(e => e.Mushok).HasPrecision(18, 2);
        builder.Property(e => e.SRateWMushok).HasPrecision(18, 2);
        builder.Property(e => e.InterestRate).HasPrecision(18, 2);
        builder.Property(e => e.InterestAmt).HasPrecision(18, 2);
        builder.Property(e => e.InsuranceAmt).HasPrecision(18, 2);

        builder.Property(e => e.Remarks).HasMaxLength(500);
        builder.Property(e => e.Compressor).HasMaxLength(200);
        builder.Property(e => e.Motor).HasMaxLength(200);
        builder.Property(e => e.Panel).HasMaxLength(200);
        builder.Property(e => e.Spareparts).HasMaxLength(200);
        builder.Property(e => e.Service).HasMaxLength(200);

        builder.HasOne(e => e.SOrder)
            .WithMany(o => o.SOrderDetails)
            .HasForeignKey(e => e.SOrderID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.StockDetail)
            .WithMany()
            .HasForeignKey(e => e.SDetailID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
