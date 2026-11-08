using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");
        builder.HasKey(e => e.StockID);

        builder.Property(e => e.StockCode).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Quantity).HasPrecision(18, 2);
        builder.Property(e => e.MRPPrice).HasPrecision(18, 2);
        builder.Property(e => e.LPPrice).HasPrecision(18, 2);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Color)
            .WithMany()
            .HasForeignKey(e => e.ColorID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Godown)
            .WithMany()
            .HasForeignKey(e => e.GodownID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.StockDetails)
            .WithOne(e => e.Stock)
            .HasForeignKey(e => e.StockID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
