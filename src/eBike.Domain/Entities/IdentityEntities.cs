using eBike.Domain.Common;

namespace eBike.Domain.Entities;

public class UserMaster : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public UserMaster User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class RolePermissions : BaseEntity
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string ModuleName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
}

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public UserMaster User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
}
