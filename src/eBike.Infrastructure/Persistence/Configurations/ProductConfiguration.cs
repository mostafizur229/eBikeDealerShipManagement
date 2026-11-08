using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.ProductID);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ProductName)
            .IsRequired();

        builder.Property(p => p.PicturePath)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(p => p.PWDiscount).HasPrecision(18, 2);
        builder.Property(p => p.MRP).HasPrecision(18, 2);
        builder.Property(p => p.VatAmount).HasPrecision(18, 2);
        builder.Property(p => p.PMRate).HasPrecision(18, 2);
        builder.Property(p => p.CommissionRate).HasPrecision(18, 2);
        builder.Property(p => p.PMushokRate).HasPrecision(18, 2);

        builder.Property(p => p.UnitType).HasConversion<int>().IsRequired(false);
        builder.Property(p => p.ProductType).HasConversion<int>();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryID)
            .IsRequired();

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyID)
            .IsRequired();

        builder.HasOne(p => p.SisterConcern)
            .WithMany()
            .HasForeignKey(p => p.ConcernID)
            .IsRequired();
    }
}
