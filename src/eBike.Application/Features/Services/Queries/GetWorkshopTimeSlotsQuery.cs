using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetWorkshopTimeSlotsQuery(bool OnlyActive = false) : IRequest<List<WorkshopTimeSlotDto>>;

public class GetWorkshopTimeSlotsQueryHandler : IRequestHandler<GetWorkshopTimeSlotsQuery, List<WorkshopTimeSlotDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkshopTimeSlotsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkshopTimeSlotDto>> Handle(GetWorkshopTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkshopTimeSlots.AsNoTracking().AsQueryable();

        if (request.OnlyActive)
        {
            query = query.Where(s => s.IsActive);
        }

        var list = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.SlotId)
            .Select(s => new WorkshopTimeSlotDto
            {
                SlotId = s.SlotId,
                SlotName = s.SlotName,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                MaxCapacity = s.MaxCapacity,
                IsActive = s.IsActive,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return list;
    }
}
