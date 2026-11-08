using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class SOrderConfiguration : IEntityTypeConfiguration<SOrder>
{
    public void Configure(EntityTypeBuilder<SOrder> builder)
    {
        builder.ToTable("SOrders");
        builder.HasKey(e => e.SOrderID);

        builder.Property(e => e.InvoiceNo).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Remarks).HasMaxLength(350);
        builder.Property(e => e.MRNo).HasMaxLength(150);
        builder.Property(e => e.LedgerNo).HasMaxLength(150);
        builder.Property(e => e.REGMRNo).HasMaxLength(150);

        builder.Property(e => e.VATPercentage).HasPrecision(18, 2);
        builder.Property(e => e.VATAmount).HasPrecision(18, 2);
        builder.Property(e => e.GrandTotal).HasPrecision(18, 2);
        builder.Property(e => e.TDPercentage).HasPrecision(18, 2);
        builder.Property(e => e.TDAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.PaymentDue).HasPrecision(18, 2);
        builder.Property(e => e.RecAmount).HasPrecision(18, 2);
        builder.Property(e => e.NetDiscount).HasPrecision(18, 2);
        builder.Property(e => e.AdjAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalDue).HasPrecision(18, 2);
        builder.Property(e => e.TotalOffer).HasPrecision(18, 2);
        builder.Property(e => e.PGrandTotal).HasPrecision(18, 2);
        builder.Property(e => e.CardPaidAmount).HasPrecision(18, 2);
        builder.Property(e => e.DepositChargePercent).HasPrecision(18, 2);
        builder.Property(e => e.TotalMushok).HasPrecision(18, 2);
        builder.Property(e => e.TotalInstAmt).HasPrecision(18, 2);
        builder.Property(e => e.TOfferAmt).HasPrecision(18, 2);
        builder.Property(e => e.PenaltyAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.SOrderDetails)
            .WithOne(e => e.SOrder)
            .HasForeignKey(e => e.SOrderID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
