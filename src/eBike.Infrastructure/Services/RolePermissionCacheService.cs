using eBike.Application.Common.Interfaces;
using eBike.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eBike.Infrastructure.Services;

public class RolePermissionCacheService : IRolePermissionCacheService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;

    public RolePermissionCacheService(ApplicationDbContext dbContext, ICacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<bool> HasPermissionAsync(int userId, string moduleName, string action, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"perm:user:{userId}:{moduleName.ToLowerInvariant()}:{action.ToLowerInvariant()}";

        var cachedResult = await _cacheService.GetAsync<bool?>(cacheKey, cancellationToken);
        if (cachedResult.HasValue)
        {
            return cachedResult.Value;
        }

        // Get user roles
        var userRoleIds = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (!userRoleIds.Any())
        {
            await _cacheService.SetAsync(cacheKey, false, TimeSpan.FromMinutes(10), cancellationToken);
            return false;
        }

        // Check if user is Admin
        var isAdmin = await _dbContext.Roles
            .AnyAsync(r => userRoleIds.Contains(r.Id) && (r.Name == "Admin" || r.Name == "SuperAdmin"), cancellationToken);

        if (isAdmin)
        {
            await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromHours(1), cancellationToken);
            return true;
        }

        // Check menu permissions via ApplicationRoleMenus and MenuItems
        var hasMenuAccess = await _dbContext.ApplicationRoleMenus
            .Where(arm => userRoleIds.Contains(arm.RoleId))
            .Join(_dbContext.MenuItems,
                arm => arm.MenuId,
                m => m.Id,
                (arm, m) => new { m.Title, m.Url })
            .AnyAsync(m => 
                m.Title.ToLower() == moduleName.ToLower() || 
                (m.Url != null && m.Url.ToLower().Contains(moduleName.ToLower())), 
                cancellationToken);

        await _cacheService.SetAsync(cacheKey, hasMenuAccess, TimeSpan.FromMinutes(15), cancellationToken);
        return hasMenuAccess;
    }

    public async Task InvalidateUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
