using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");
        builder.HasKey(d => d.DistrictID);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasMany(d => d.Thanas)
            .WithOne(t => t.District)
            .HasForeignKey(t => t.DistrictID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
