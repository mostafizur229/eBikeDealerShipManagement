using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Designations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Designations.Queries;

public record GetDesignationsQuery(string? Search = null) : IRequest<List<DesignationListDto>>;

public class GetDesignationsQueryHandler : IRequestHandler<GetDesignationsQuery, List<DesignationListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDesignationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DesignationListDto>> Handle(GetDesignationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Designations
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d =>
                (d.Description != null && d.Description.ToLower().Contains(search)) ||
                (d.Code != null && d.Code.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(d => d.Code)
            .Select(d => new DesignationListDto
            {
                DesignationID = d.DesignationID,
                Code = d.Code,
                Description = d.Description,
                CommissionPercent = d.CommissionPercent
            })
            .ToListAsync(cancellationToken);
    }
}
