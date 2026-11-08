using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record DeleteWorkshopChecklistMasterCommand(int ItemId) : IRequest<bool>;

public class DeleteWorkshopChecklistMasterCommandHandler 
    : IRequestHandler<DeleteWorkshopChecklistMasterCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkshopChecklistMasterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteWorkshopChecklistMasterCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkshopChecklistMasters
            .FirstOrDefaultAsync(x => x.ItemId == request.ItemId, cancellationToken);

        if (entity == null) return false;

        _context.WorkshopChecklistMasters.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
