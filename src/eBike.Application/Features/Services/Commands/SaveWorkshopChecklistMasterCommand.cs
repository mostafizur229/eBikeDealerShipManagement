using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public class SaveWorkshopChecklistMasterCommand : IRequest<int>
{
    public int? ItemId { get; set; }
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public string ToDoAction { get; set; } = "Inspect";
    public string? IntervalNotes { get; set; }
    public string? DefaultRemarks { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class SaveWorkshopChecklistMasterCommandHandler 
    : IRequestHandler<SaveWorkshopChecklistMasterCommand, int>
{
    private readonly IApplicationDbContext _context;

    public SaveWorkshopChecklistMasterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        SaveWorkshopChecklistMasterCommand request, 
        CancellationToken cancellationToken)
    {
        WorkshopChecklistMaster? entity = null;

        if (request.ItemId.HasValue && request.ItemId.Value > 0)
        {
            entity = await _context.WorkshopChecklistMasters
                .FirstOrDefaultAsync(x => x.ItemId == request.ItemId.Value, cancellationToken);
        }

        if (entity == null)
        {
            entity = new WorkshopChecklistMaster
            {
                Category = request.Category,
                Description = request.Description,
                ToDoAction = request.ToDoAction,
                IntervalNotes = request.IntervalNotes,
                DefaultRemarks = request.DefaultRemarks,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _context.WorkshopChecklistMasters.Add(entity);
        }
        else
        {
            entity.Category = request.Category;
            entity.Description = request.Description;
            entity.ToDoAction = request.ToDoAction;
            entity.IntervalNotes = request.IntervalNotes;
            entity.DefaultRemarks = request.DefaultRemarks;
            entity.DisplayOrder = request.DisplayOrder;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entity.ItemId;
    }
}
