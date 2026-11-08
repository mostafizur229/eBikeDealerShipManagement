using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesReturns.Commands;

public record DeleteSalesReturnCommand(int Id) : IRequest;

public class DeleteSalesReturnCommandHandler : IRequestHandler<DeleteSalesReturnCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSalesReturnCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var sOrder = await _context.SOrders
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.StockDetail)
            .FirstOrDefaultAsync(s => s.SOrderID == request.Id, cancellationToken);

        if (sOrder == null)
            throw new NotFoundException($"Sales Return with ID {request.Id} not found.");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == sOrder.CustomerID, cancellationToken);

        if (customer != null)
        {
            customer.TotalDue += sOrder.PaymentDue;
        }

        foreach (var detail in sOrder.SOrderDetails)
        {
            if (detail.SDetailID > 0)
            {
                var stockDetail = await _context.StockDetails
                    .FirstOrDefaultAsync(sd => sd.SDetailID == detail.SDetailID, cancellationToken);

                if (stockDetail != null)
                {
                    stockDetail.Status = (int)EnumStockStatus.Sold; // Re-mark as sold
                }

                int godownId = stockDetail?.GodownID ?? 0;
                int? colorId = stockDetail?.ColorID;

                var parentStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductID == detail.ProductID &&
                                              s.GodownID == godownId &&
                                              (colorId == null || s.ColorID == colorId) &&
                                              s.ConcernID == sOrder.ConcernID, cancellationToken);

                if (parentStock != null)
                {
                    parentStock.Quantity = Math.Max(0, parentStock.Quantity - detail.Quantity);
                }
            }
        }

        _context.SOrderDetails.RemoveRange(sOrder.SOrderDetails);
        _context.SOrders.Remove(sOrder);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
