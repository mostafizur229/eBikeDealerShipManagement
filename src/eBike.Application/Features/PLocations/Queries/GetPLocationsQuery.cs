using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PLocations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PLocations.Queries;

public record GetPLocationsQuery(
    string? Search = null,
    int? ConcernId = null,
    int? Limit = null
) : IRequest<List<PLocationDto>>;

public class GetPLocationsQueryHandler : IRequestHandler<GetPLocationsQuery, List<PLocationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPLocationsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PLocationDto>> Handle(GetPLocationsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.PLocations
            .Include(p => p.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(p => p.ConcernID == targetConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Code.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        query = query.OrderBy(p => p.Description).ThenBy(p => p.Code);

        if (request.Limit.HasValue && request.Limit.Value > 0)
        {
            query = query.Take(request.Limit.Value);
        }

        return await query
            .Select(p => new PLocationDto
            {
                Id = p.PLocationID,
                Code = p.Code,
                Description = p.Description,
                ConcernId = p.ConcernID,
                ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
