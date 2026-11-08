using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Commands;

public record DeleteSalesOrderCommand(int Id) : IRequest<bool>;

public class DeleteSalesOrderCommandHandler : IRequestHandler<DeleteSalesOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISalesOrderExecutionService _salesOrderService;

    public DeleteSalesOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISalesOrderExecutionService salesOrderService)
    {
        _context = context;
        _currentUser = currentUser;
        _salesOrderService = salesOrderService;
    }

    public async Task<bool> Handle(DeleteSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SOrderID == request.Id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(SOrder), request.Id);
        }

        var detailIds = await _context.SOrderDetails
            .AsNoTracking()
            .Where(d => d.SOrderID == request.Id)
            .Select(d => d.SOrderDetailID)
            .ToListAsync(cancellationToken);

        if (detailIds.Count > 0)
        {
            var registrations = await _context.Registrations
                .Where(r => r.SorderDetailID.HasValue && detailIds.Contains(r.SorderDetailID.Value))
                .ToListAsync(cancellationToken);

            if (registrations.Count > 0)
            {
                _context.Registrations.RemoveRange(registrations);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        int userId = _currentUser.UserId ?? 0;
        return await _salesOrderService.DeleteSalesOrderAsync(request.Id, userId, cancellationToken);
    }
}
