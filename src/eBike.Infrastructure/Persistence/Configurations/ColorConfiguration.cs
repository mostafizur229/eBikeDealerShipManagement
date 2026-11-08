using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        builder.ToTable("Colors");
        builder.HasKey(c => c.ColorID);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasOne(c => c.SisterConcern)
            .WithMany()
            .HasForeignKey(c => c.ConcernID)
            .IsRequired();
    }
}
