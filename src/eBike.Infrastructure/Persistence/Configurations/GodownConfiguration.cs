using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class GodownConfiguration : IEntityTypeConfiguration<Godown>
{
    public void Configure(EntityTypeBuilder<Godown> builder)
    {
        builder.ToTable("Godowns");
        builder.HasKey(g => g.GodownID);

        builder.Property(g => g.Code)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(g => g.Name)
            .IsRequired();

        builder.Property(g => g.Address)
            .IsRequired(false);

        builder.Property(g => g.ISCommon)
            .IsRequired();

        builder.HasOne(g => g.SisterConcern)
            .WithMany()
            .HasForeignKey(g => g.ConcernID)
            .IsRequired();
    }
}
