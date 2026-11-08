using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Thanas.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Queries;

public record GetThanasByDistrictIdQuery(int DistrictId) : IRequest<List<ThanaListDto>>;

public class GetThanasByDistrictIdQueryHandler : IRequestHandler<GetThanasByDistrictIdQuery, List<ThanaListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetThanasByDistrictIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ThanaListDto>> Handle(GetThanasByDistrictIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Thanas
            .AsNoTracking()
            .Where(t => t.DistrictID == request.DistrictId)
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
