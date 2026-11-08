using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class SystemInformationConfiguration : IEntityTypeConfiguration<SystemInformation>
{
    public void Configure(EntityTypeBuilder<SystemInformation> builder)
    {
        builder.ToTable("SystemInformations");
        builder.HasKey(s => s.SystemInfoID);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(250);
        builder.Property(s => s.Address).IsRequired().HasMaxLength(255);
        builder.Property(s => s.TelephoneNo).HasMaxLength(255);
        builder.Property(s => s.EmailAddress).HasMaxLength(255);
        builder.Property(s => s.WebAddress).HasMaxLength(255);
        builder.Property(s => s.smsCharge).HasColumnType("decimal(18,2)");

        builder.HasOne(s => s.SisterConcern)
            .WithMany()
            .HasForeignKey(s => s.ConcernID)
            .IsRequired(false);
    }
}
