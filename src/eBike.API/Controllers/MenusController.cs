using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Menus.DTOs;
using eBike.Application.Features.Menus.Queries;
using eBike.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBike.API.Controllers;

[Authorize]
public class MenusController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public MenusController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns dynamic navigation groups and permitted menu items for the authenticated user.
    /// </summary>
    [HttpGet("nav")]
    public async Task<ActionResult<IReadOnlyList<NavigationGroupDto>>> GetNavigationMenu(CancellationToken cancellationToken)
    {
        if (CurrentUserId == 0)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var result = await Sender.Send(new GetNavigationMenuQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists all menu items from MenuItems table.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMenus([FromQuery] string? search, CancellationToken ct)
    {
        var rawMenus = await _context.MenuItems
            .AsNoTracking()
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            rawMenus = rawMenus.Where(m => (m.Title != null && m.Title.ToLower().Contains(s)) || 
                                           (m.Url != null && m.Url.ToLower().Contains(s)) ||
                                           (m.Description != null && m.Description.ToLower().Contains(s))).ToList();
        }

        var lookup = rawMenus.ToDictionary(m => m.Id, m => m.Title);

        var menus = rawMenus
            .OrderBy(m => (m.ParentId.HasValue && m.ParentId.Value > 0) ? m.ParentId.Value : m.Id)
            .ThenBy(m => m.Sequence)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.Description,
                m.ParentId,
                ParentTitle = (m.ParentId.HasValue && m.ParentId.Value > 0 && lookup.TryGetValue(m.ParentId.Value, out var pt)) ? pt : null,
                m.Url,
                m.WithoutView,
                m.Sequence,
                m.Icon
            })
            .ToList();

        return Ok(menus);
    }

    /// <summary>
    /// Returns root menu categories for parent selection dropdowns.
    /// </summary>
    [HttpGet("parents")]
    public async Task<IActionResult> GetParentCategories(CancellationToken ct)
    {
        var parents = await _context.MenuItems
            .AsNoTracking()
            .Where(m => !m.ParentId.HasValue || m.ParentId.Value == 0)
            .OrderBy(m => m.Sequence)
            .Select(m => new { m.Id, m.Title, m.Sequence })
            .ToListAsync(ct);

        return Ok(parents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMenuById(int id, CancellationToken ct)
    {
        var menu = await _context.MenuItems
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (menu == null)
            return NotFound(new { message = $"Menu with ID {id} was not found." });

        string? parentTitle = null;
        if (menu.ParentId.HasValue && menu.ParentId.Value > 0)
        {
            parentTitle = await _context.MenuItems
                .Where(m => m.Id == menu.ParentId.Value)
                .Select(m => m.Title)
                .FirstOrDefaultAsync(ct);
        }

        return Ok(new
        {
            menu.Id,
            menu.Title,
            menu.Description,
            menu.ParentId,
            ParentTitle = parentTitle,
            menu.Url,
            menu.WithoutView,
            menu.Sequence,
            menu.Icon
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new { message = "Title is required." });

        var menu = new ApplicationMenuItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            ParentId = (dto.ParentId.HasValue && dto.ParentId.Value > 0) ? dto.ParentId.Value : null,
            Url = dto.Url?.Trim(),
            WithoutView = dto.WithoutView,
            Sequence = dto.Sequence,
            Icon = dto.Icon?.Trim()
        };

        _context.MenuItems.Add(menu);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetMenuById), new { id = menu.Id }, new { id = menu.Id, message = "Menu created successfully." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMenu(int id, [FromBody] UpdateMenuDto dto, CancellationToken ct)
    {
        var menu = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (menu == null)
            return NotFound(new { message = $"Menu with ID {id} was not found." });

        if (!string.IsNullOrWhiteSpace(dto.Title))
            menu.Title = dto.Title.Trim();

        menu.Description = dto.Description?.Trim();
        menu.ParentId = (dto.ParentId.HasValue && dto.ParentId.Value > 0) ? dto.ParentId.Value : null;
        menu.Url = dto.Url?.Trim();
        menu.WithoutView = dto.WithoutView;
        menu.Sequence = dto.Sequence;
        menu.Icon = dto.Icon?.Trim();

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Menu updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMenu(int id, CancellationToken ct)
    {
        var menu = await _context.MenuItems
            .Include(m => m.Children)
            .Include(m => m.RoleMenus)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (menu == null)
            return NotFound(new { message = $"Menu with ID {id} was not found." });

        if (menu.Children.Any())
            return BadRequest(new { message = "Cannot delete a menu item that has child submenus. Delete or reassign children first." });

        _context.ApplicationRoleMenus.RemoveRange(menu.RoleMenus);
        _context.MenuItems.Remove(menu);
        await _context.SaveChangesAsync(ct);

        return Ok(new { message = "Menu deleted successfully." });
    }
}

public record CreateMenuDto(
    string Title,
    string? Description,
    int? ParentId,
    string? Url,
    bool WithoutView,
    int Sequence,
    string? Icon
);

public record UpdateMenuDto(
    string? Title,
    string? Description,
    int? ParentId,
    string? Url,
    bool WithoutView,
    int Sequence,
    string? Icon
);
