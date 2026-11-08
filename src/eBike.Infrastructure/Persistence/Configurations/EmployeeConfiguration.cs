using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.EmployeeID);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Name)
            .IsRequired();

        builder.Property(e => e.GrossSalary)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.BasicSalary)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.SRDueLimit)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.Designation)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DesignationID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Religion)
            .WithMany(r => r.Employees)
            .HasForeignKey(e => e.ReligionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SisterConcern)
            .WithMany()
            .HasForeignKey(e => e.ConcernID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
