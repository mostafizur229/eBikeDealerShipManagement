namespace eBike.Domain.Entities;

public class UserRefreshToken
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public Guid TokenFamilyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public string? UserAgent { get; set; }
    public bool IsRevoked { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
