using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ExpenseItemConfiguration : IEntityTypeConfiguration<ExpenseItem>
{
    public void Configure(EntityTypeBuilder<ExpenseItem> builder)
    {
        builder.ToTable("ExpenseItems");
        builder.HasKey(e => e.ExpenseItemID);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ConcernID)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Ignore(e => e.Id);
        builder.Ignore(e => e.Name);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .IsRequired();
    }
}
