using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class SaleOfferConfiguration : IEntityTypeConfiguration<SaleOffer>
{
    public void Configure(EntityTypeBuilder<SaleOffer> builder)
    {
        builder.ToTable("SaleOffers");
        builder.HasKey(e => e.OfferID);

        builder.Property(e => e.OfferCode).HasMaxLength(250);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.OfferValue).HasPrecision(18, 2);
        builder.Property(e => e.ThresholdValue).HasPrecision(18, 2);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
