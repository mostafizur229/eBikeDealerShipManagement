using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CreditInterestPercentageConfiguration : IEntityTypeConfiguration<CreditInterestPercentage>
{
    public void Configure(EntityTypeBuilder<CreditInterestPercentage> builder)
    {
        builder.ToTable("CreditInterestPercentages");
        builder.HasKey(e => e.IntPercentageID);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Percentage)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
