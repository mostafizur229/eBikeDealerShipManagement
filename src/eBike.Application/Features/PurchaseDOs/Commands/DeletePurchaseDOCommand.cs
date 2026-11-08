using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseDOs.Commands;

public record DeletePurchaseDOCommand(int Id) : IRequest<bool>;

public class DeletePurchaseDOCommandHandler : IRequestHandler<DeletePurchaseDOCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeletePurchaseDOCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeletePurchaseDOCommand request, CancellationToken cancellationToken)
    {
        var purchaseDO = await _context.DOes
            .Include(d => d.DODetails)
            .FirstOrDefaultAsync(d => d.DOID == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Purchase DO with ID {request.Id} was not found.");

        _context.DOes.Remove(purchaseDO);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
