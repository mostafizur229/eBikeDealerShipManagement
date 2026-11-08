using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.ToTable("BankTransactions");
        builder.HasKey(bt => bt.BankTranID);

        builder.Property(bt => bt.TranDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(bt => bt.TransactionNo)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(bt => bt.TransactionType)
            .IsRequired();

        builder.Property(bt => bt.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(bt => bt.ChecqueNo)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(bt => bt.Remarks)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(bt => bt.WFStatus)
            .IsRequired();

        builder.HasOne(bt => bt.Bank)
            .WithMany()
            .HasForeignKey(bt => bt.BankID)
            .IsRequired(false);

        builder.HasOne(bt => bt.AnotherBank)
            .WithMany()
            .HasForeignKey(bt => bt.AnotherBankID)
            .IsRequired(false);

        builder.HasOne(bt => bt.Customer)
            .WithMany()
            .HasForeignKey(bt => bt.CustomerID)
            .IsRequired(false);

        builder.HasOne(bt => bt.Supplier)
            .WithMany()
            .HasForeignKey(bt => bt.SupplierID)
            .IsRequired(false);

        builder.HasOne(bt => bt.SisterConcern)
            .WithMany()
            .HasForeignKey(bt => bt.ConcernID)
            .IsRequired(false);
    }
}
