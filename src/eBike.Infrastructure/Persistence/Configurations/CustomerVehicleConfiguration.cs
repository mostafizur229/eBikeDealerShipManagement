using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CustomerVehicleConfiguration : IEntityTypeConfiguration<CustomerVehicle>
{
    public void Configure(EntityTypeBuilder<CustomerVehicle> builder)
    {
        builder.ToTable("CustomerVehicles");
        builder.HasKey(v => v.VehicleID);

        builder.Property(v => v.ChassisNo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.EngineNo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.OriginType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(v => v.CurrentOdometerReading)
            .HasColumnType("decimal(10,2)");

        builder.HasOne(v => v.Customer)
            .WithMany()
            .HasForeignKey(v => v.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Product)
            .WithMany()
            .HasForeignKey(v => v.ProductID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Color)
            .WithMany()
            .HasForeignKey(v => v.ColorID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.SOrder)
            .WithMany()
            .HasForeignKey(v => v.SOrderID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.SisterConcern)
            .WithMany()
            .HasForeignKey(v => v.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JobCardConfiguration : IEntityTypeConfiguration<JobCard>
{
    public void Configure(EntityTypeBuilder<JobCard> builder)
    {
        builder.ToTable("JobCards");
        builder.HasKey(j => j.JobCardID);

        builder.Property(j => j.JobCardNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(j => j.OdometerReading)
            .HasColumnType("decimal(10,2)");

        builder.Property(j => j.PartsTotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.LaborTotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.SubTotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.VATAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.GrandTotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(j => j.DueAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(j => j.Customer)
            .WithMany()
            .HasForeignKey(j => j.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.Vehicle)
            .WithMany(v => v.JobCards)
            .HasForeignKey(j => j.VehicleID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JobCardLaborDetailConfiguration : IEntityTypeConfiguration<JobCardLaborDetail>
{
    public void Configure(EntityTypeBuilder<JobCardLaborDetail> builder)
    {
        builder.ToTable("JobCardLaborDetails");
        builder.HasKey(l => l.LaborDetailID);

        builder.Property(l => l.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Discount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(l => l.JobCard)
            .WithMany(j => j.LaborDetails)
            .HasForeignKey(l => l.JobCardID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JobCardPartsDetailConfiguration : IEntityTypeConfiguration<JobCardPartsDetail>
{
    public void Configure(EntityTypeBuilder<JobCardPartsDetail> builder)
    {
        builder.ToTable("JobCardPartsDetails");
        builder.HasKey(p => p.PartsDetailID);

        builder.Property(p => p.Quantity).HasColumnType("decimal(10,2)");
        builder.Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Discount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.JobCard)
            .WithMany(j => j.PartsDetails)
            .HasForeignKey(p => p.JobCardID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
