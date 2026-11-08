using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Districts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Districts.Queries;

public record GetDistrictsQuery(string? Search = null) : IRequest<List<DistrictListDto>>;

public class GetDistrictsQueryHandler : IRequestHandler<GetDistrictsQuery, List<DistrictListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDistrictsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DistrictListDto>> Handle(GetDistrictsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Districts
            .AsNoTracking()
            .Include(d => d.Thanas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d =>
                (d.Name != null && d.Name.ToLower().Contains(search)) ||
                (d.Code != null && d.Code.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(d => d.Name)
            .Select(d => new DistrictListDto
            {
                DistrictID = d.DistrictID,
                Code = d.Code,
                Name = d.Name,
                ThanaCount = d.Thanas.Count
            })
            .ToListAsync(cancellationToken);
    }
}
