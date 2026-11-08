using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Godowns.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Godowns.Queries;

public record GetGodownsQuery(
    string? SearchTerm = null,
    int? ConcernId = null,
    bool? OnlyCommon = null
) : IRequest<List<GodownDto>>;

public class GetGodownsQueryHandler : IRequestHandler<GetGodownsQuery, List<GodownDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetGodownsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<GodownDto>> Handle(GetGodownsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Godowns
            .Include(g => g.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        int? filterConcernId = request.ConcernId ?? _currentUser.ConcernId;

        if (request.OnlyCommon == true)
        {
            query = query.Where(g => g.ISCommon == 1);
        }
        else if (filterConcernId.HasValue && filterConcernId.Value > 0)
        {
            // Show godowns belonging to user's concern OR shared/common godowns
            query = query.Where(g => g.ConcernID == filterConcernId.Value || g.ISCommon == 1);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(g =>
                g.Name.ToLower().Contains(search) ||
                g.Code.ToLower().Contains(search) ||
                (g.Address != null && g.Address.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(g => g.Code)
            .Select(g => new GodownDto
            {
                Id = g.GodownID,
                Code = g.Code,
                Name = g.Name,
                Address = g.Address,
                ConcernId = g.ConcernID,
                ConcernName = g.SisterConcern != null ? g.SisterConcern.Name : null,
                IsCommon = g.ISCommon == 1
            })
            .ToListAsync(cancellationToken);
    }
}
