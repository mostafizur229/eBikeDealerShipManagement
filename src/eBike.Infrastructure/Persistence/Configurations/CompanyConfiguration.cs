using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.CompanyID);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(350);

        builder.HasOne(c => c.SisterConcern)
            .WithMany()
            .HasForeignKey(c => c.ConcernID)
            .IsRequired();
    }
}
