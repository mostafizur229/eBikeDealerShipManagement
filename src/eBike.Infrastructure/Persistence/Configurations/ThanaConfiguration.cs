using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ThanaConfiguration : IEntityTypeConfiguration<Thana>
{
    public void Configure(EntityTypeBuilder<Thana> builder)
    {
        builder.ToTable("Thanas");
        builder.HasKey(t => t.ThanaID);

        builder.Property(t => t.Code)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasOne(t => t.District)
            .WithMany(d => d.Thanas)
            .HasForeignKey(t => t.DistrictID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
