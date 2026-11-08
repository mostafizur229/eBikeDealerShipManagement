using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ServiceFollowUpLogConfiguration : IEntityTypeConfiguration<ServiceFollowUpLog>
{
    public void Configure(EntityTypeBuilder<ServiceFollowUpLog> builder)
    {
        builder.ToTable("ServiceFollowUpLogs");
        builder.HasKey(e => e.FollowUpLogID);

        builder.Property(e => e.CallStatus).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ChassisNo).HasMaxLength(100);
        builder.Property(e => e.CallerName).HasMaxLength(100);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Vehicle)
            .WithMany()
            .HasForeignKey(e => e.VehicleID)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
