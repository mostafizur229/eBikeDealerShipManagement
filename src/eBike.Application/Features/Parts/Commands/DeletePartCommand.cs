using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Parts.Commands;

public record DeletePartCommand(int Id) : IRequest;

public class DeletePartCommandHandler : IRequestHandler<DeletePartCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _context.Parts
            .FirstOrDefaultAsync(p => p.PartsNoID == request.Id, cancellationToken);

        if (part == null)
        {
            throw new NotFoundException(nameof(Part), request.Id);
        }

        int usageCount = await _context.Products
            .CountAsync(p => p.PartsNoID == request.Id, cancellationToken);

        if (usageCount > 0)
        {
            throw new ConflictException($"Cannot delete Part No '{part.Description}' because it is assigned to {usageCount} product(s).");
        }

        _context.Parts.Remove(part);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
