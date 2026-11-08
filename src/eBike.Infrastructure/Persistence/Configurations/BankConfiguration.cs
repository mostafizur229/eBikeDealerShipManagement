using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("Banks");
        builder.HasKey(b => b.BankID);

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.BankName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.AccountNo)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.BranchName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.AccountName)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.OpeningBalance)
            .HasPrecision(18, 2);

        builder.Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.IsAdvancedDueLimitApplicable)
            .IsRequired(false);

        builder.Property(b => b.AdvancedAmountLimit)
            .HasPrecision(18, 2)
            .IsRequired(false);

        builder.HasOne(b => b.SisterConcern)
            .WithMany()
            .HasForeignKey(b => b.ConcernID)
            .IsRequired();
    }
}
