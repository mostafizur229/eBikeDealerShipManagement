using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetWorkshopChecklistMastersQuery(bool OnlyActive = false, string? Category = null) 
    : IRequest<List<WorkshopChecklistMasterDto>>;

public class GetWorkshopChecklistMastersQueryHandler 
    : IRequestHandler<GetWorkshopChecklistMastersQuery, List<WorkshopChecklistMasterDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkshopChecklistMastersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkshopChecklistMasterDto>> Handle(
        GetWorkshopChecklistMastersQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.WorkshopChecklistMasters.AsNoTracking().AsQueryable();

        if (request.OnlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(x => x.Category == request.Category);
        }

        return await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ItemId)
            .Select(x => new WorkshopChecklistMasterDto
            {
                ItemId = x.ItemId,
                Category = x.Category,
                Description = x.Description,
                ToDoAction = x.ToDoAction,
                IntervalNotes = x.IntervalNotes,
                DefaultRemarks = x.DefaultRemarks,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
