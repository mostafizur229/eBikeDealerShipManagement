using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CommissionSetupConfiguration : IEntityTypeConfiguration<CommissionSetup>
{
    public void Configure(EntityTypeBuilder<CommissionSetup> builder)
    {
        builder.ToTable("CommissionSetups");
        builder.HasKey(e => e.CSID);

        builder.Property(e => e.AchievedPercentStart).HasPrecision(18, 2);
        builder.Property(e => e.AchievedPercentEnd).HasPrecision(18, 2);
        builder.Property(e => e.CommisssionAmt).HasPrecision(18, 2);
        builder.Property(e => e.CommissionPercent).HasPrecision(18, 2);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DesWiseCommissionConfiguration : IEntityTypeConfiguration<DesWiseCommission>
{
    public void Configure(EntityTypeBuilder<DesWiseCommission> builder)
    {
        builder.ToTable("DesWiseCommissions");
        builder.HasKey(e => e.ID);

        builder.Property(e => e.CommissionPercent).HasPrecision(18, 2);

        builder.HasOne(e => e.Designation)
            .WithMany()
            .HasForeignKey(e => e.DesignationID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
