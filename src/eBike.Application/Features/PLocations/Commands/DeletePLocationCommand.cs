using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PLocations.Commands;

public record DeletePLocationCommand(int Id) : IRequest;

public class DeletePLocationCommandHandler : IRequestHandler<DeletePLocationCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePLocationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.PLocations
            .FirstOrDefaultAsync(p => p.PLocationID == request.Id, cancellationToken);

        if (location == null)
        {
            throw new NotFoundException(nameof(PLocation), request.Id);
        }

        int usageCount = await _context.Products
            .CountAsync(p => p.PLocationID == request.Id, cancellationToken);

        if (usageCount > 0)
        {
            throw new ConflictException($"Cannot delete Rack Location '{location.Description}' because it is assigned to {usageCount} product(s).");
        }

        _context.PLocations.Remove(location);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
