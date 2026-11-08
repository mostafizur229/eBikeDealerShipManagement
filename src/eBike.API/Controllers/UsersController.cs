using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBike.API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly ICryptographyService _cryptoService;

    public UsersController(IApplicationDbContext context, ICryptographyService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    /// <summary>
    /// Lists users filtered ConcernWise. Operators only see users belonging to their assigned concern.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int? concernId, CancellationToken ct)
    {
        var targetConcernId = CurrentConcernId > 0 ? CurrentConcernId : (concernId ?? 0);

        var query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.SisterConcern)
            .Include(u => u.Employee)
            .AsQueryable();

        if (targetConcernId > 0)
        {
            query = query.Where(u => u.ConcernID == targetConcernId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.UserName.ToLower().Contains(s) || 
                                     (u.Email != null && u.Email.ToLower().Contains(s)) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
        }

        var users = await query
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                u.ConcernID,
                ConcernName = u.SisterConcern != null ? u.SisterConcern.Name : null,
                u.EmployeeID,
                EmployeeName = u.Employee != null ? u.Employee.Name : null,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                RoleIds = u.UserRoles.Select(ur => ur.RoleId).ToList(),
                u.LockoutEnabled,
                u.AccessFailedCount
            })
            .ToListAsync(ct);

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.SisterConcern)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
            return NotFound(new { message = $"User with ID {id} was not found." });

        // Security check: if current user has a concern assigned, ensure they can only inspect their concern's users
        if (CurrentConcernId > 0 && user.ConcernID != CurrentConcernId)
            return Forbid();

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.ConcernID,
            ConcernName = user.SisterConcern != null ? user.SisterConcern.Name : null,
            user.EmployeeID,
            EmployeeName = user.Employee != null ? user.Employee.Name : null,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            RoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList(),
            user.LockoutEnabled
        });
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> GetUserLookup(CancellationToken ct)
    {
        var targetConcernId = CurrentConcernId;

        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name })
            .ToListAsync(ct);

        var concernsQuery = _context.SisterConcerns.AsNoTracking();
        if (targetConcernId > 0)
        {
            concernsQuery = concernsQuery.Where(c => c.ConcernID == targetConcernId);
        }
        var concerns = await concernsQuery
            .OrderBy(c => c.Name)
            .Select(c => new { c.ConcernID, c.Name })
            .ToListAsync(ct);

        var empQuery = _context.Employees.AsNoTracking();
        var employees = await empQuery
            .OrderBy(e => e.Name)
            .Select(e => new { e.EmployeeID, e.Name, e.ContactNo })
            .ToListAsync(ct);

        return Ok(new { roles, concerns, employees, currentConcernId = targetConcernId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName))
            return BadRequest(new { message = "Username is required." });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Password is required for a new user." });

        var exists = await _context.Users.AnyAsync(u => u.UserName.ToLower() == dto.UserName.Trim().ToLower(), ct);
        if (exists)
            return BadRequest(new { message = $"Username '{dto.UserName}' is already in use." });

        var (hash, salt) = _cryptoService.CreatePasswordHash(dto.Password);

        var assignedConcernId = CurrentConcernId > 0 ? CurrentConcernId : dto.ConcernID;

        var user = new ApplicationUser
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email?.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            PasswordHash = hash,
            SecurityStamp = salt,
            ConcernID = assignedConcernId,
            EmployeeID = dto.EmployeeID,
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        if (dto.RoleIds != null && dto.RoleIds.Count > 0)
        {
            foreach (var roleId in dto.RoleIds.Distinct())
            {
                _context.UserRoles.Add(new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
            await _context.SaveChangesAsync(ct);
        }

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, new { id = user.Id, message = "User created successfully." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
            return NotFound(new { message = $"User with ID {id} was not found." });

        if (CurrentConcernId > 0 && user.ConcernID != CurrentConcernId)
            return Forbid();

        user.Email = dto.Email?.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.ConcernID = CurrentConcernId > 0 ? CurrentConcernId : dto.ConcernID;
        user.EmployeeID = dto.EmployeeID;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var (hash, salt) = _cryptoService.CreatePasswordHash(dto.Password);
            user.PasswordHash = hash;
            user.SecurityStamp = salt;
        }

        if (dto.RoleIds != null)
        {
            _context.UserRoles.RemoveRange(user.UserRoles);
            foreach (var roleId in dto.RoleIds.Distinct())
            {
                _context.UserRoles.Add(new ApplicationUserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "User updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
            return NotFound(new { message = $"User with ID {id} was not found." });

        if (CurrentConcernId > 0 && user.ConcernID != CurrentConcernId)
            return Forbid();

        if (user.Id == CurrentUserId)
            return BadRequest(new { message = "You cannot delete your own logged-in user account." });

        _context.UserRoles.RemoveRange(user.UserRoles);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);

        return Ok(new { message = "User deleted successfully." });
    }
}

public record CreateUserDto(
    string UserName,
    string? Email,
    string? PhoneNumber,
    string Password,
    int ConcernID,
    int EmployeeID,
    List<int>? RoleIds
);

public record UpdateUserDto(
    string? Email,
    string? PhoneNumber,
    string? Password,
    int ConcernID,
    int EmployeeID,
    List<int>? RoleIds
);
