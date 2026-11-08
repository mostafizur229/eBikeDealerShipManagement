using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.ToTable("Designations");
        builder.HasKey(d => d.DesignationID);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Description)
            .IsRequired();

        builder.Property(d => d.CommissionPercent)
            .HasColumnType("decimal(18,2)");
    }
}
