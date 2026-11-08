using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditSalesOrders.Commands;

public record DeleteCreditSalesOrderCommand(int Id) : IRequest<bool>;

public class DeleteCreditSalesOrderCommandHandler : IRequestHandler<DeleteCreditSalesOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICreditSalesOrderExecutionService _creditSalesOrderService;

    public DeleteCreditSalesOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICreditSalesOrderExecutionService creditSalesOrderService)
    {
        _context = context;
        _currentUser = currentUser;
        _creditSalesOrderService = creditSalesOrderService;
    }

    public async Task<bool> Handle(DeleteCreditSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.CreditSales
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreditSalesID == request.Id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(CreditSale), request.Id);
        }

        var detailIds = await _context.CreditSaleDetails
            .AsNoTracking()
            .Where(d => d.CreditSalesID == request.Id)
            .Select(d => d.CreditSaleDetailsID)
            .ToListAsync(cancellationToken);

        if (detailIds.Count > 0)
        {
            var registrations = await _context.Registrations
                .Where(r => r.CreditSaleDetailsID.HasValue && detailIds.Contains(r.CreditSaleDetailsID.Value))
                .ToListAsync(cancellationToken);

            if (registrations.Count > 0)
            {
                _context.Registrations.RemoveRange(registrations);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        int userId = _currentUser.UserId ?? 0;
        return await _creditSalesOrderService.DeleteCreditSalesOrderAsync(request.Id, userId, cancellationToken);
    }
}
