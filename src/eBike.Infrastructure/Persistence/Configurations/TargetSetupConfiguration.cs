using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class TargetSetupConfiguration : IEntityTypeConfiguration<TargetSetup>
{
    public void Configure(EntityTypeBuilder<TargetSetup> builder)
    {
        builder.ToTable("TargetSetups");
        builder.HasKey(e => e.TID);

        builder.Property(e => e.Quantity).HasPrecision(18, 2);
        builder.Property(e => e.Amount).HasPrecision(18, 2);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.TargetSetupDetails)
            .WithOne(d => d.TargetSetup)
            .HasForeignKey(d => d.TID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TargetSetupDetailConfiguration : IEntityTypeConfiguration<TargetSetupDetail>
{
    public void Configure(EntityTypeBuilder<TargetSetupDetail> builder)
    {
        builder.ToTable("TargetSetupDetails");
        builder.HasKey(e => e.ID);

        builder.Property(e => e.Quantity).HasPrecision(18, 2);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
