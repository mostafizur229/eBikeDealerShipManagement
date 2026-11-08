using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.CustomerID);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Name)
            .IsRequired();

        builder.Property(c => c.TotalDue)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.CreditDue)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.OpeningDue)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.CusDueLimit)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(c => c.District)
            .WithMany()
            .HasForeignKey(c => c.DistrictID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ThanaObj)
            .WithMany()
            .HasForeignKey(c => c.ThanaID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Employee)
            .WithMany()
            .HasForeignKey(c => c.EmployeeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.SisterConcern)
            .WithMany()
            .HasForeignKey(c => c.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.ServiceTickets)
            .WithOne(s => s.Customer)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
