using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CreditSalesScheduleConfiguration : IEntityTypeConfiguration<CreditSalesSchedule>
{
    public void Configure(EntityTypeBuilder<CreditSalesSchedule> builder)
    {
        builder.ToTable("CreditSalesSchedules");
        builder.HasKey(e => e.CSScheduleID);

        builder.Property(e => e.PaymentStatus).HasMaxLength(300).IsRequired();

        builder.Property(e => e.Balance).HasPrecision(19, 4);
        builder.Property(e => e.InstallmentAmt).HasPrecision(19, 4);
        builder.Property(e => e.InterestAmount).HasPrecision(19, 4);
        builder.Property(e => e.ClosingBalance).HasPrecision(19, 4);
        builder.Property(e => e.HireValue).HasPrecision(18, 2);
        builder.Property(e => e.NetValue).HasPrecision(18, 2);
        builder.Property(e => e.ExpectedInstallment).HasPrecision(18, 2);
        builder.Property(e => e.LastPayAdjust).HasPrecision(18, 2);
        builder.Property(e => e.CardPaidAmount).HasPrecision(18, 2);
        builder.Property(e => e.DepositChargePercent).HasPrecision(18, 2);
        builder.Property(e => e.LastPayAdjustment).HasPrecision(18, 2);

        builder.HasOne(e => e.CreditSale)
            .WithMany(s => s.CreditSalesSchedules)
            .HasForeignKey(e => e.CreditSalesID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
