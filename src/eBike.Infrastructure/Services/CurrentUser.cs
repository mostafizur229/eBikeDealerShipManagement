using System.Security.Claims;
using eBike.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace eBike.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User?.FindFirst("sub")?.Value 
                          ?? User?.FindFirst("userId")?.Value;

            return int.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value 
                               ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

    public int? ConcernId
    {
        get
        {
            var val = User?.FindFirst("concernId")?.Value
                      ?? User?.FindFirst("ConcernId")?.Value
                      ?? User?.FindFirst("concernID")?.Value
                      ?? User?.FindFirst("ConcernID")?.Value;
            return int.TryParse(val, out var cid) ? cid : null;
        }
    }

    public int? EmployeeId
    {
        get
        {
            var val = User?.FindFirst("employeeId")?.Value;
            return int.TryParse(val, out var eid) ? eid : null;
        }
    }

    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
