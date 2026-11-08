namespace eBike.Application.Features.Auth.DTOs;

public class UserMenuDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public string? Url { get; set; }
    public int Sequence { get; set; }
    public string? Icon { get; set; }
    public List<UserMenuDto> Children { get; set; } = new();
}

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<UserMenuDto> Menus { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
