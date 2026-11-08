using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.SupplierID);

        builder.Property(s => s.Code)
            .HasMaxLength(50);

        builder.Property(s => s.Name);

        builder.Property(s => s.OwnerName);

        builder.Property(s => s.ContactNo)
            .HasMaxLength(150);

        builder.Property(s => s.Address);

        builder.Property(s => s.PhotoPath)
            .HasMaxLength(250);

        builder.Property(s => s.TotalDue)
            .HasPrecision(18, 2);

        builder.Property(s => s.OpeningDue)
            .HasPrecision(18, 2);

        builder.Property(s => s.Remarks);

        builder.HasOne(s => s.SisterConcern)
            .WithMany()
            .HasForeignKey(s => s.ConcernID)
            .IsRequired();
    }
}
