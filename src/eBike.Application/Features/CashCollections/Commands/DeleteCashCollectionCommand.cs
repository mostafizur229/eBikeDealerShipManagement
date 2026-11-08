using eBike.Application.Common.Interfaces;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Commands;

public record DeleteCashCollectionCommand(int Id) : IRequest<bool>;

public class DeleteCashCollectionCommandHandler : IRequestHandler<DeleteCashCollectionCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICashDueService _cashDueService;

    public DeleteCashCollectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICashDueService cashDueService)
    {
        _context = context;
        _currentUser = currentUser;
        _cashDueService = cashDueService;
    }

    public async Task<bool> Handle(DeleteCashCollectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CashCollections
            .FirstOrDefaultAsync(c => c.CashCollectionID == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cash Collection with ID {request.Id} not found.");

        if (_currentUser.ConcernId.HasValue && entity.ConcernID != _currentUser.ConcernId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete records outside your concern.");
        }

        if (entity.CustomerID.HasValue && entity.CustomerID.Value > 0)
        {
            decimal netCollectionAmount;
            if (entity.TransactionType == (int)EnumTranType.CollectionReturn)
            {
                netCollectionAmount = -((entity.Amount ?? 0m) + entity.AdjustAmt);
            }
            else
            {
                netCollectionAmount = (entity.Amount ?? 0m) + entity.AdjustAmt - entity.InterestAmt;
            }

            await _cashDueService.UpdateTotalDueAsync(entity.CustomerID.Value, 0, -netCollectionAmount, cancellationToken);
        }

        _context.CashCollections.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
