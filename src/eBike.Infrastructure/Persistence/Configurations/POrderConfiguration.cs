using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class POrderConfiguration : IEntityTypeConfiguration<POrder>
{
    public void Configure(EntityTypeBuilder<POrder> builder)
    {
        builder.ToTable("POrders");
        builder.HasKey(e => e.POrderID);

        builder.Property(e => e.ChallanNo).HasMaxLength(150).IsRequired();
        builder.Property(e => e.GrandTotal).HasPrecision(18, 2);
        builder.Property(e => e.TDiscount).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmt).HasPrecision(18, 2);
        builder.Property(e => e.RecAmt).HasPrecision(18, 2);
        builder.Property(e => e.PaymentDue).HasPrecision(18, 2);
        builder.Property(e => e.TotalDue).HasPrecision(18, 2);
        builder.Property(e => e.PPDisAmt).HasPrecision(18, 2);
        builder.Property(e => e.NetDiscount).HasPrecision(18, 2);
        builder.Property(e => e.LaborCost).HasPrecision(18, 2);
        builder.Property(e => e.AdjAmount).HasPrecision(18, 2);
        builder.Property(e => e.TotalMushok).HasPrecision(18, 2);
        builder.Property(e => e.TotalTax).HasPrecision(18, 2);

        builder.HasOne(e => e.Supplier)
            .WithMany()
            .HasForeignKey(e => e.SupplierID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.POrderDetails)
            .WithOne(e => e.POrder)
            .HasForeignKey(e => e.POrderID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
