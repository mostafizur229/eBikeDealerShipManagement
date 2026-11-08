using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBike.API.Controllers;

[Authorize]
public class RolesController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public RolesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                UserCount = r.UserRoles.Count,
                MenuCount = r.RoleMenus.Count
            })
            .ToListAsync(ct);

        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id, CancellationToken ct)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RoleMenus)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role == null)
            return NotFound(new { message = $"Role with ID {id} was not found." });

        return Ok(new
        {
            role.Id,
            role.Name,
            MenuIds = role.RoleMenus.Select(rm => rm.MenuId).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Role name is required." });

        var exists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == dto.Name.Trim().ToLower(), ct);
        if (exists)
            return BadRequest(new { message = $"Role '{dto.Name}' already exists." });

        var role = new ApplicationRole
        {
            Name = dto.Name.Trim()
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);

        if (dto.MenuIds != null && dto.MenuIds.Count > 0)
        {
            foreach (var menuId in dto.MenuIds.Distinct())
            {
                _context.ApplicationRoleMenus.Add(new ApplicationRoleMenu
                {
                    RoleId = role.Id,
                    MenuId = menuId
                });
            }
            await _context.SaveChangesAsync(ct);
        }

        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, new { id = role.Id, message = "Role created successfully." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto, CancellationToken ct)
    {
        var role = await _context.Roles
            .Include(r => r.RoleMenus)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role == null)
            return NotFound(new { message = $"Role with ID {id} was not found." });

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var exists = await _context.Roles.AnyAsync(r => r.Id != id && r.Name.ToLower() == dto.Name.Trim().ToLower(), ct);
            if (exists)
                return BadRequest(new { message = $"Role '{dto.Name}' already exists." });

            role.Name = dto.Name.Trim();
        }

        if (dto.MenuIds != null)
        {
            _context.ApplicationRoleMenus.RemoveRange(role.RoleMenus);
            foreach (var menuId in dto.MenuIds.Distinct())
            {
                _context.ApplicationRoleMenus.Add(new ApplicationRoleMenu
                {
                    RoleId = role.Id,
                    MenuId = menuId
                });
            }
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Role updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken ct)
    {
        var role = await _context.Roles
            .Include(r => r.RoleMenus)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (role == null)
            return NotFound(new { message = $"Role with ID {id} was not found." });

        if (role.UserRoles.Any())
            return BadRequest(new { message = "Cannot delete a role that is currently assigned to users. Reassign users first." });

        _context.ApplicationRoleMenus.RemoveRange(role.RoleMenus);
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(ct);

        return Ok(new { message = "Role deleted successfully." });
    }
}

public record CreateRoleDto(string Name, List<int>? MenuIds);
public record UpdateRoleDto(string? Name, List<int>? MenuIds);
