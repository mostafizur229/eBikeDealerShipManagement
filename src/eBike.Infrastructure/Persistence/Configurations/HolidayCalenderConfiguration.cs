using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class HolidayCalenderConfiguration : IEntityTypeConfiguration<HolidayCalender>
{
    public void Configure(EntityTypeBuilder<HolidayCalender> builder)
    {
        builder.ToTable("HolidayCalenders");
        builder.HasKey(e => e.ID);

        builder.Property(e => e.Description).HasMaxLength(250);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
