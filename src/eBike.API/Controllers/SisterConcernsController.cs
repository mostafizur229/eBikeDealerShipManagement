using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eBike.API.Controllers;

[Authorize]
public class SisterConcernsController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public SisterConcernsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets only the concern that is currently logged in, and its child concerns (if any).
    /// Dealership users are not allowed to create new concerns.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSisterConcerns([FromQuery] string? search, CancellationToken ct)
    {
        var targetConcernId = CurrentConcernId;

        var query = _context.SisterConcerns.AsNoTracking().AsQueryable();

        if (targetConcernId > 0)
        {
            // Strictly scoped to currently logged in concern and its child concerns
            query = query.Where(sc => sc.ConcernID == targetConcernId || sc.ParentID == targetConcernId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(sc => sc.Name.ToLower().Contains(s) ||
                                      (sc.Address != null && sc.Address.ToLower().Contains(s)) ||
                                      (sc.ContactNo != null && sc.ContactNo.Contains(s)));
        }

        var concerns = await query
            .OrderBy(sc => sc.ConcernID == targetConcernId ? 0 : 1)
            .ThenBy(sc => sc.Name)
            .Select(sc => new
            {
                sc.ConcernID,
                sc.Name,
                sc.Address,
                sc.ContactNo,
                sc.SmsContactNo,
                sc.ParentID,
                IsCurrentConcern = sc.ConcernID == targetConcernId,
                sc.SalesShowPercent,
                sc.PurchaseShowPercent,
                sc.StockShowPercent,
                sc.ServiceCharge
            })
            .ToListAsync(ct);

        return Ok(concerns);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var targetConcernId = CurrentConcernId;
        if (targetConcernId > 0 && id != targetConcernId)
        {
            var isChild = await _context.SisterConcerns.AnyAsync(sc => sc.ConcernID == id && sc.ParentID == targetConcernId, ct);
            if (!isChild) return Forbid();
        }

        var concern = await _context.SisterConcerns
            .AsNoTracking()
            .FirstOrDefaultAsync(sc => sc.ConcernID == id, ct);

        if (concern == null) return NotFound(new { message = $"Concern with ID {id} not found." });

        return Ok(concern);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSisterConcernDto dto, CancellationToken ct)
    {
        var targetConcernId = CurrentConcernId;
        if (targetConcernId > 0 && id != targetConcernId)
        {
            var isChild = await _context.SisterConcerns.AnyAsync(sc => sc.ConcernID == id && sc.ParentID == targetConcernId, ct);
            if (!isChild) return Forbid();
        }

        var concern = await _context.SisterConcerns.FirstOrDefaultAsync(sc => sc.ConcernID == id, ct);
        if (concern == null) return NotFound(new { message = $"Concern with ID {id} not found." });

        if (!string.IsNullOrWhiteSpace(dto.Name)) concern.Name = dto.Name.Trim();
        concern.Address = dto.Address?.Trim();
        concern.ContactNo = dto.ContactNo?.Trim();
        concern.SmsContactNo = dto.SmsContactNo?.Trim();

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Concern details updated successfully." });
    }
}

public class UpdateSisterConcernDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
    public string? SmsContactNo { get; set; }
}
