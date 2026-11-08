using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record SaveWorkshopTimeSlotCommand(
    int? SlotId,
    string SlotName,
    string StartTime,
    string EndTime,
    int MaxCapacity,
    bool IsActive,
    int DisplayOrder
) : IRequest<int>;

public class SaveWorkshopTimeSlotCommandHandler : IRequestHandler<SaveWorkshopTimeSlotCommand, int>
{
    private readonly IApplicationDbContext _context;

    public SaveWorkshopTimeSlotCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SaveWorkshopTimeSlotCommand request, CancellationToken cancellationToken)
    {
        WorkshopTimeSlot? slot = null;

        if (request.SlotId.HasValue && request.SlotId.Value > 0)
        {
            slot = await _context.WorkshopTimeSlots.FirstOrDefaultAsync(s => s.SlotId == request.SlotId.Value, cancellationToken);
        }

        if (slot == null)
        {
            slot = new WorkshopTimeSlot
            {
                SlotName = request.SlotName.Trim(),
                StartTime = request.StartTime.Trim(),
                EndTime = request.EndTime.Trim(),
                MaxCapacity = request.MaxCapacity > 0 ? request.MaxCapacity : 4,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1,
                ConcernID = 3056
            };
            _context.WorkshopTimeSlots.Add(slot);
        }
        else
        {
            slot.SlotName = request.SlotName.Trim();
            slot.StartTime = request.StartTime.Trim();
            slot.EndTime = request.EndTime.Trim();
            slot.MaxCapacity = request.MaxCapacity > 0 ? request.MaxCapacity : 4;
            slot.IsActive = request.IsActive;
            slot.DisplayOrder = request.DisplayOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return slot.SlotId;
    }
}

public record DeleteWorkshopTimeSlotCommand(int SlotId) : IRequest<bool>;

public class DeleteWorkshopTimeSlotCommandHandler : IRequestHandler<DeleteWorkshopTimeSlotCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkshopTimeSlotCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteWorkshopTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _context.WorkshopTimeSlots.FirstOrDefaultAsync(s => s.SlotId == request.SlotId, cancellationToken);
        if (slot == null) return false;

        _context.WorkshopTimeSlots.Remove(slot);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
