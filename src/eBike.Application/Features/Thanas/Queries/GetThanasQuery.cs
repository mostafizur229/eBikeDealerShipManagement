using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Thanas.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Queries;

public record GetThanasQuery(
    string? Search = null,
    int? DistrictId = null
) : IRequest<List<ThanaListDto>>;

public class GetThanasQueryHandler : IRequestHandler<GetThanasQuery, List<ThanaListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetThanasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ThanaListDto>> Handle(GetThanasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Thanas
            .AsNoTracking()
            .Include(t => t.District)
            .AsQueryable();

        if (request.DistrictId.HasValue && request.DistrictId > 0)
        {
            query = query.Where(t => t.DistrictID == request.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(t =>
                (t.Name != null && t.Name.ToLower().Contains(search)) ||
                (t.Code != null && t.Code.ToLower().Contains(search)) ||
                (t.District != null && t.District.Name.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new ThanaListDto
            {
                ThanaID = t.ThanaID,
                Code = t.Code,
                Name = t.Name,
                DistrictID = t.DistrictID,
                DistrictName = t.District != null ? t.District.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
