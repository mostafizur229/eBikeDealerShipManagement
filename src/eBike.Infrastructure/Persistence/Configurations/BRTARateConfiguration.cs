using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class BRTARateConfiguration : IEntityTypeConfiguration<BRTARate>
{
    public void Configure(EntityTypeBuilder<BRTARate> builder)
    {
        builder.ToTable("BRTARates");
        builder.HasKey(e => e.ID);

        builder.Property(e => e.CC).HasPrecision(18, 2);
        builder.Property(e => e.NewRegMF).HasPrecision(18, 2);
        builder.Property(e => e.NewRegIF).HasPrecision(18, 2);
        builder.Property(e => e.IssueTaxTokeMF).HasPrecision(18, 2);
        builder.Property(e => e.RegCertificateMF).HasPrecision(18, 2);
        builder.Property(e => e.SupplementaryMF).HasPrecision(18, 2);
        builder.Property(e => e.NamePlate).HasPrecision(18, 2);
        builder.Property(e => e.Contribution).HasPrecision(18, 2);

        builder.Ignore(e => e.TotalAmount);
    }
}

