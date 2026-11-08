using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class POProductDetailConfiguration : IEntityTypeConfiguration<POProductDetail>
{
    public void Configure(EntityTypeBuilder<POProductDetail> builder)
    {
        builder.ToTable("POProductDetails");
        builder.HasKey(e => e.POPDID);

        builder.Property(e => e.IMENO).IsRequired();
        builder.Property(e => e.EngineNo).IsRequired();
        builder.Property(e => e.KeyNo).IsRequired();
        builder.Property(e => e.SerialNo).HasMaxLength(550);
        builder.Property(e => e.ReturnQty).HasPrecision(18, 2);

        builder.HasOne(e => e.POrderDetail)
            .WithMany(d => d.POProductDetails)
            .HasForeignKey(e => e.POrderDetailID)
            .OnDelete(DeleteBehavior.Cascade);

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
