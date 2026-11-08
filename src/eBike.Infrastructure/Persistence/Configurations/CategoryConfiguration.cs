using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categorys");
        builder.HasKey(c => c.CategoryID);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .IsRequired();

        builder.HasOne(c => c.SisterConcern)
            .WithMany()
            .HasForeignKey(c => c.ConcernID)
            .IsRequired();
    }
}
