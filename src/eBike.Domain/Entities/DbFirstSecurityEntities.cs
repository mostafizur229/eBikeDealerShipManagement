namespace eBike.Domain.Entities;

public class ApplicationUser
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PasswordHash { get; set; }
    public string? SecurityStamp { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LockoutEndDateUtc { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ConcernID { get; set; }
    public int EmployeeID { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();
    public SisterConcern? SisterConcern { get; set; }
    public Employee? Employee { get; set; }
}

public class ApplicationRole
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<ApplicationRoleMenu> RoleMenus { get; set; } = new List<ApplicationRoleMenu>();
}

public class ApplicationUserRole
{
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
}

public class ApplicationMenuItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? Url { get; set; }
    public bool WithoutView { get; set; }
    public int Sequence { get; set; }
    public string? Icon { get; set; }

    public ApplicationMenuItem? Parent { get; set; }
    public ICollection<ApplicationMenuItem> Children { get; set; } = new List<ApplicationMenuItem>();
    public ICollection<ApplicationRoleMenu> RoleMenus { get; set; } = new List<ApplicationRoleMenu>();
}

public class ApplicationRoleMenu
{
    public int RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;

    public int MenuId { get; set; }
    public ApplicationMenuItem Menu { get; set; } = null!;
}

public class ApplicationPermission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ApplicationMenuPermission
{
    public int RoleMenuId { get; set; }
    public int PermissionId { get; set; }
}

public class SisterConcern
{
    public int ConcernID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
    public int ParentID { get; set; }
    public decimal SalesShowPercent { get; set; }
    public decimal PurchaseShowPercent { get; set; }
    public decimal StockShowPercent { get; set; }
    public decimal ServiceCharge { get; set; }
    public string? SmsContactNo { get; set; }
}
