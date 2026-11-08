using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseOrders.Commands;

public record DeletePurchaseOrderCommand(int Id) : IRequest<bool>;

public class DeletePurchaseOrderCommandHandler : IRequestHandler<DeletePurchaseOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPurchaseOrderExecutionService _poService;

    public DeletePurchaseOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPurchaseOrderExecutionService poService)
    {
        _context = context;
        _currentUser = currentUser;
        _poService = poService;
    }

    public async Task<bool> Handle(DeletePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.POrders
            .FirstOrDefaultAsync(p => p.POrderID == request.Id, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Purchase Order with ID {request.Id} not found.");

        if (order.Status == 2) 
            throw new BadRequestException("This Purchase Order has already been returned/deleted.");

        int soldCount = await _poService.CheckProductStatusByPOIdAsync(request.Id, cancellationToken);
        if (soldCount > 0)
        {
            throw new BadRequestException("Some product(s) have already been sold from this order. This order cannot be deleted or returned.");
        }

        int userId = _currentUser.UserId ?? 0;
        bool success = await _poService.ReturnPurchaseOrderAsync(request.Id, userId, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Failed to return/delete Purchase Order in database.");
        }

        return true;
    }
}
