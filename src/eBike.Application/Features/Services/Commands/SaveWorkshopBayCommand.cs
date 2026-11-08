using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record SaveWorkshopBayCommand(
    int? BayId,
    string BayName,
    string BayType,
    string? Description,
    int Capacity,
    bool IsActive,
    int DisplayOrder
) : IRequest<int>;

public class SaveWorkshopBayCommandHandler : IRequestHandler<SaveWorkshopBayCommand, int>
{
    private readonly IApplicationDbContext _context;

    public SaveWorkshopBayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SaveWorkshopBayCommand request, CancellationToken cancellationToken)
    {
        WorkshopBay? bay = null;

        if (request.BayId.HasValue && request.BayId.Value > 0)
        {
            bay = await _context.WorkshopBays.FirstOrDefaultAsync(b => b.BayId == request.BayId.Value, cancellationToken);
        }

        if (bay == null)
        {
            bay = new WorkshopBay
            {
                BayName = request.BayName.Trim(),
                BayType = string.IsNullOrWhiteSpace(request.BayType) ? "Hydraulic Ramp" : request.BayType.Trim(),
                Description = request.Description?.Trim(),
                Capacity = request.Capacity > 0 ? request.Capacity : 1,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder > 0 ? request.DisplayOrder : 1,
                ConcernID = 3056
            };
            _context.WorkshopBays.Add(bay);
        }
        else
        {
            bay.BayName = request.BayName.Trim();
            bay.BayType = string.IsNullOrWhiteSpace(request.BayType) ? bay.BayType : request.BayType.Trim();
            bay.Description = request.Description?.Trim();
            bay.Capacity = request.Capacity > 0 ? request.Capacity : 1;
            bay.IsActive = request.IsActive;
            bay.DisplayOrder = request.DisplayOrder;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return bay.BayId;
    }
}

public record DeleteWorkshopBayCommand(int BayId) : IRequest<bool>;

public class DeleteWorkshopBayCommandHandler : IRequestHandler<DeleteWorkshopBayCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkshopBayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteWorkshopBayCommand request, CancellationToken cancellationToken)
    {
        var bay = await _context.WorkshopBays.FirstOrDefaultAsync(b => b.BayId == request.BayId, cancellationToken);
        if (bay == null) return false;

        _context.WorkshopBays.Remove(bay);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
