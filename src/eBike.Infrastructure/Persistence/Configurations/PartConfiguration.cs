using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("Parts");
        builder.HasKey(p => p.PartsNoID);

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
