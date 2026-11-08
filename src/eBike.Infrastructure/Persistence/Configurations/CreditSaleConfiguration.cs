using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CreditSaleConfiguration : IEntityTypeConfiguration<CreditSale>
{
    public void Configure(EntityTypeBuilder<CreditSale> builder)
    {
        builder.ToTable("CreditSales");
        builder.HasKey(e => e.CreditSalesID);

        builder.Property(e => e.InvoiceNo).HasMaxLength(150).IsRequired();
        builder.Property(e => e.UserName).HasMaxLength(250);
        builder.Property(e => e.InstallmentPeriod).HasMaxLength(250);
        builder.Property(e => e.BChallanNo).HasMaxLength(250);
        builder.Property(e => e.MRNo).HasMaxLength(150);
        builder.Property(e => e.LedgerNo).HasMaxLength(150);
        builder.Property(e => e.REGMRNo).HasMaxLength(150);

        builder.Property(e => e.TSalesAmt).HasPrecision(19, 4);
        builder.Property(e => e.IntallmentPrinciple).HasPrecision(19, 4);
        builder.Property(e => e.Remaining).HasPrecision(18, 2);
        builder.Property(e => e.DownPayment).HasPrecision(19, 4);
        builder.Property(e => e.WInterestAmt).HasPrecision(19, 4);
        builder.Property(e => e.FixedAmt).HasPrecision(19, 4);
        builder.Property(e => e.Discount).HasPrecision(19, 4);
        builder.Property(e => e.NetAmount).HasPrecision(19, 4);
        builder.Property(e => e.VATPercentage).HasPrecision(18, 2);
        builder.Property(e => e.VATAmount).HasPrecision(18, 2);
        builder.Property(e => e.InterestRate).HasPrecision(6, 2);
        builder.Property(e => e.InterestAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalOffer).HasPrecision(18, 2);
        builder.Property(e => e.PGrandTotal).HasPrecision(18, 2);
        builder.Property(e => e.PenaltyInterest).HasPrecision(18, 2);
        builder.Property(e => e.LastPayAdjAmt).HasPrecision(18, 2);
        builder.Property(e => e.CardPaidAmount).HasPrecision(18, 2);
        builder.Property(e => e.DepositChargePercent).HasPrecision(18, 2);
        builder.Property(e => e.MergeTotalSales).HasPrecision(18, 2);
        builder.Property(e => e.TOfferAmt).HasPrecision(18, 2);
        builder.Property(e => e.TotalMushok).HasPrecision(18, 2);
        builder.Property(e => e.TExAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.CreditSaleDetails)
            .WithOne(e => e.CreditSale)
            .HasForeignKey(e => e.CreditSalesID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.CreditSalesSchedules)
            .WithOne(e => e.CreditSale)
            .HasForeignKey(e => e.CreditSalesID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
