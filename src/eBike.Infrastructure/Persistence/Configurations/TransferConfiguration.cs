using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");
        builder.HasKey(t => t.TransferID);

        builder.Property(t => t.TransferNo).HasMaxLength(100).IsRequired();
        builder.Property(t => t.TotalAmount).HasPrecision(18, 2);
        builder.Property(t => t.Remarks).HasMaxLength(500);

        builder.HasOne(t => t.FromConcern)
            .WithMany()
            .HasForeignKey(t => t.FromConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToConcern)
            .WithMany()
            .HasForeignKey(t => t.ToConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.TransferDetails)
            .WithOne(d => d.Transfer)
            .HasForeignKey(d => d.TransferID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TransferDetailConfiguration : IEntityTypeConfiguration<TransferDetail>
{
    public void Configure(EntityTypeBuilder<TransferDetail> builder)
    {
        builder.ToTable("TransferDetails");
        builder.HasKey(d => d.TDetailID);

        builder.Property(d => d.PRate).HasPrecision(18, 2);
        builder.Property(d => d.Quantity).HasPrecision(18, 2);
        builder.Property(d => d.UTAmount).HasPrecision(18, 2);
        builder.Property(d => d.IMEI).HasMaxLength(100);

        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ToGodown)
            .WithMany()
            .HasForeignKey(d => d.ToGodownID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ToColor)
            .WithMany()
            .HasForeignKey(d => d.ToColorID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TransferHistoryConfiguration : IEntityTypeConfiguration<TransferHistory>
{
    public void Configure(EntityTypeBuilder<TransferHistory> builder)
    {
        builder.ToTable("TransferHistory");
        builder.HasKey(h => h.TransferHID);

        builder.Property(h => h.Qty).HasPrecision(18, 2);

        builder.HasOne(h => h.Product)
            .WithMany()
            .HasForeignKey(h => h.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.FromGodownNavigation)
            .WithMany()
            .HasForeignKey(h => h.FromGodown)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.ToGodownNavigation)
            .WithMany()
            .HasForeignKey(h => h.ToGodown)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
