using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registrations");
        builder.HasKey(e => e.RegistrationID);

        builder.Property(e => e.RegNo).HasMaxLength(150);
        builder.Property(e => e.REGMRNo).HasMaxLength(150);

        builder.Property(e => e.RegFee).HasPrecision(18, 2);
        builder.Property(e => e.InsuranceAmt).HasPrecision(18, 2);
        builder.Property(e => e.BRTATIPS).HasPrecision(18, 2);
        builder.Property(e => e.BRTARegFee).HasPrecision(18, 2);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SOrderDetail)
            .WithMany()
            .HasForeignKey(e => e.SorderDetailID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreditSaleDetail)
            .WithMany()
            .HasForeignKey(e => e.CreditSaleDetailsID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.NewCustomer)
            .WithMany()
            .HasForeignKey(e => e.NewCusID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
