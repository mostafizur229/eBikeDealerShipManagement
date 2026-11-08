using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetWorkshopBaysQuery(bool OnlyActive = false) : IRequest<List<WorkshopBayDto>>;

public class GetWorkshopBaysQueryHandler : IRequestHandler<GetWorkshopBaysQuery, List<WorkshopBayDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkshopBaysQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkshopBayDto>> Handle(GetWorkshopBaysQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkshopBays.AsNoTracking().AsQueryable();

        if (request.OnlyActive)
        {
            query = query.Where(b => b.IsActive);
        }

        var list = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.BayId)
            .Select(b => new WorkshopBayDto
            {
                BayId = b.BayId,
                BayName = b.BayName,
                BayType = b.BayType,
                Description = b.Description,
                Capacity = b.Capacity,
                IsActive = b.IsActive,
                DisplayOrder = b.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return list;
    }
}
