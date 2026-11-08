using eBike.Application.Common.Interfaces;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashDeliveries.Commands;

public record DeleteCashDeliveryCommand(int Id) : IRequest<bool>;

public class DeleteCashDeliveryCommandHandler : IRequestHandler<DeleteCashDeliveryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICashDueService _cashDueService;

    public DeleteCashDeliveryCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICashDueService cashDueService)
    {
        _context = context;
        _currentUser = currentUser;
        _cashDueService = cashDueService;
    }

    public async Task<bool> Handle(DeleteCashDeliveryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CashCollections
            .FirstOrDefaultAsync(c => c.CashCollectionID == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cash Delivery with ID {request.Id} not found.");

        if (_currentUser.ConcernId.HasValue && entity.ConcernID != _currentUser.ConcernId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete records outside your concern.");
        }

        if (entity.TransactionType == (int)EnumTranType.ToCompany && entity.SupplierID.HasValue && entity.SupplierID.Value > 0)
        {
            decimal totalAmt = (entity.Amount ?? 0m) + entity.AdjustAmt;
            await _cashDueService.UpdateTotalDueAsync(0, entity.SupplierID.Value, -totalAmt, cancellationToken);
        }

        _context.CashCollections.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
