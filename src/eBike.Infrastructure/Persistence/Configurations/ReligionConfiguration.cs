using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ReligionConfiguration : IEntityTypeConfiguration<Religion>
{
    public void Configure(EntityTypeBuilder<Religion> builder)
    {
        builder.ToTable("Religions");
        builder.HasKey(r => r.ReligionID);

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
