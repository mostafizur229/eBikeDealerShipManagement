using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class PurchaseDOConfiguration : IEntityTypeConfiguration<PurchaseDO>
{
    public void Configure(EntityTypeBuilder<PurchaseDO> builder)
    {
        builder.ToTable("DOes");
        builder.HasKey(e => e.DOID);

        builder.Property(e => e.DONo).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Remarks).HasMaxLength(500);
        builder.Property(e => e.TotalAmt).HasPrecision(18, 2);
        builder.Property(e => e.PaidAmt).HasPrecision(18, 2);
        builder.Property(e => e.NetDiscount).HasPrecision(18, 2);
        builder.Property(e => e.TotalTax).HasPrecision(18, 2);
        builder.Property(e => e.GrandTotal).HasPrecision(18, 2);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany()
            .HasForeignKey(e => e.SupplierID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.DODetails)
            .WithOne(e => e.PurchaseDO)
            .HasForeignKey(e => e.DOID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseDODetailConfiguration : IEntityTypeConfiguration<PurchaseDODetail>
{
    public void Configure(EntityTypeBuilder<PurchaseDODetail> builder)
    {
        builder.ToTable("DODetails");
        builder.HasKey(e => e.DODID);

        builder.Property(e => e.DOQty).HasPrecision(18, 2);
        builder.Property(e => e.GivenQty).HasPrecision(18, 2);
        builder.Property(e => e.MRP).HasPrecision(18, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmt).HasPrecision(18, 2);
        builder.Property(e => e.PPDisPercent).HasPrecision(18, 2);
        builder.Property(e => e.PPDisAmt).HasPrecision(18, 2);
        builder.Property(e => e.PPTaxAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Color)
            .WithMany()
            .HasForeignKey(e => e.ColorID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
