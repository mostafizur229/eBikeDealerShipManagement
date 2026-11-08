using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CashCollectionConfiguration : IEntityTypeConfiguration<CashCollection>
{
    public void Configure(EntityTypeBuilder<CashCollection> builder)
    {
        builder.ToTable("CashCollections");
        builder.HasKey(c => c.CashCollectionID);

        builder.Property(c => c.ReceiptNo)
            .HasMaxLength(300);

        builder.Property(c => c.BankName)
            .HasMaxLength(250);

        builder.Property(c => c.BranchName)
            .HasMaxLength(250);

        builder.Property(c => c.AccountNo)
            .HasMaxLength(350);

        builder.Property(c => c.MBAccountNo)
            .HasMaxLength(350);

        builder.Property(c => c.BKashNo)
            .HasMaxLength(350);

        builder.Property(c => c.Remarks)
            .HasMaxLength(350);

        builder.Property(c => c.MRNo)
            .HasMaxLength(150);

        builder.Property(c => c.Amount)
            .HasPrecision(18, 2);

        builder.Property(c => c.AdjustAmt)
            .HasPrecision(18, 2);

        builder.Property(c => c.BalanceDue)
            .HasPrecision(18, 2);

        builder.Property(c => c.InterestAmt)
            .HasPrecision(18, 2);

        builder.HasOne(c => c.SisterConcern)
            .WithMany()
            .HasForeignKey(c => c.ConcernID)
            .IsRequired();

        builder.HasOne(c => c.Supplier)
            .WithMany()
            .HasForeignKey(c => c.SupplierID)
            .IsRequired(false);

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerID)
            .IsRequired(false);
    }
}
