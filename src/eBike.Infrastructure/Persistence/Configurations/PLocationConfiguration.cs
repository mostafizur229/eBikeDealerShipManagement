using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class PLocationConfiguration : IEntityTypeConfiguration<PLocation>
{
    public void Configure(EntityTypeBuilder<PLocation> builder)
    {
        builder.ToTable("PLocations");
        builder.HasKey(p => p.PLocationID);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.HasOne(p => p.SisterConcern)
            .WithMany()
            .HasForeignKey(p => p.ConcernID)
            .IsRequired();
    }
}
