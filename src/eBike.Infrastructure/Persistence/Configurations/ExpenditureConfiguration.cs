using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ExpenditureConfiguration : IEntityTypeConfiguration<Expenditure>
{
    public void Configure(EntityTypeBuilder<Expenditure> builder)
    {
        builder.ToTable("Expenditures");
        builder.HasKey(e => e.ExpenditureID);

        builder.Property(e => e.EntryDate)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.ExpenseItemID)
            .IsRequired();

        builder.Property(e => e.VoucherNo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.ConcernID)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.CustomerType)
            .IsRequired();

        builder.Property(e => e.WFStatus)
            .IsRequired();

        builder.Ignore(e => e.Id);

        builder.HasOne(e => e.ExpenseItem)
            .WithMany()
            .HasForeignKey(e => e.ExpenseItemID)
            .IsRequired();

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .IsRequired();

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .IsRequired(false);
    }
}
