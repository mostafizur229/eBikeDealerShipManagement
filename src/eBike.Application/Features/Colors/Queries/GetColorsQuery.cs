using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Colors.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Colors.Queries;

public record GetColorsQuery(
    string? SearchTerm = null,
    int? ConcernId = null
) : IRequest<List<ColorDto>>;

public class GetColorsQueryHandler : IRequestHandler<GetColorsQuery, List<ColorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetColorsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ColorDto>> Handle(GetColorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Colors
            .Include(c => c.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        int? filterConcernId = request.ConcernId ?? _currentUser.ConcernId;
        if (filterConcernId.HasValue && filterConcernId.Value > 0)
        {
            query = query.Where(c => c.ConcernID == filterConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search));
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new ColorDto
            {
                Id = c.ColorID,
                Code = c.Code,
                Name = c.Name,
                ConcernId = c.ConcernID,
                ConcernName = c.SisterConcern != null ? c.SisterConcern.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
