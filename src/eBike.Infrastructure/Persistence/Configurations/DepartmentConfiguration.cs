using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.DepartmentId);

        builder.Property(d => d.CODE)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.DESCRIPTION)
            .IsRequired()
            .HasMaxLength(150);
    }
}
