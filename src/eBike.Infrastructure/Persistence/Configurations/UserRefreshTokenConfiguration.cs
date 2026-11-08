using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eBike.Infrastructure.Persistence.Configurations;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.Property(e => e.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        // Computed column in SQL Server
        builder.Property(e => e.IsRevoked)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(e => e.TokenHash)
            .IsUnique()
            .HasDatabaseName("UQ_UserRefreshTokens_TokenHash");

        builder.HasIndex(e => new { e.UserId, e.TokenFamilyId })
            .HasDatabaseName("IX_UserRefreshTokens_UserId_Family")
            .IncludeProperties(e => new { e.ExpiresAt, e.RevokedAt });

        builder.HasOne(e => e.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_UserRefreshTokens_Users");
    }
}
