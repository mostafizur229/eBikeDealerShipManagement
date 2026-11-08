using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Auth.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Auth.Queries;

public record GetUserMenusQuery(int UserId) : IRequest<List<UserMenuDto>>;

public class GetUserMenusQueryHandler : IRequestHandler<GetUserMenusQuery, List<UserMenuDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserMenusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserMenuDto>> Handle(GetUserMenusQuery request, CancellationToken cancellationToken)
    {
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == request.UserId)
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);

        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        bool isAdmin = userRoles.Any(ur => ur.Role != null && 
            (ur.Role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
             ur.Role.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)));

        List<ApplicationMenuItem> menuItems;
        if (isAdmin)
        {
            menuItems = await _context.MenuItems
                .AsNoTracking()
                .OrderBy(m => m.Sequence)
                .ToListAsync(cancellationToken);
        }
        else
        {
            menuItems = await _context.ApplicationRoleMenus
                .Where(arm => roleIds.Contains(arm.RoleId))
                .Select(arm => arm.Menu)
                .Distinct()
                .OrderBy(m => m.Sequence)
                .ToListAsync(cancellationToken);
        }

        return BuildMenuTree(menuItems);
    }

    private static List<UserMenuDto> BuildMenuTree(List<ApplicationMenuItem> items)
    {
        var lookup = items.ToDictionary(m => m.Id, m => new UserMenuDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            ParentId = m.ParentId,
            Url = m.Url,
            Sequence = m.Sequence,
            Icon = m.Icon
        });

        var rootMenus = new List<UserMenuDto>();

        foreach (var item in items)
        {
            var dto = lookup[item.Id];
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
            else
            {
                rootMenus.Add(dto);
            }
        }

        return rootMenus.OrderBy(m => m.Sequence).ToList();
    }
}
