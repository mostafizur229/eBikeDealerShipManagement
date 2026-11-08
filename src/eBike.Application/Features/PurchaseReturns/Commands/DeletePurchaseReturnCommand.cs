using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseReturns.Commands;

public record DeletePurchaseReturnCommand(int Id) : IRequest;

public class DeletePurchaseReturnCommandHandler : IRequestHandler<DeletePurchaseReturnCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePurchaseReturnCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        var pOrder = await _context.POrders
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.POProductDetails)
            .FirstOrDefaultAsync(p => p.POrderID == request.Id, cancellationToken);

        if (pOrder == null)
            throw new NotFoundException($"Purchase Return with ID {request.Id} not found.");

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == pOrder.SupplierID, cancellationToken);

        if (supplier != null)
        {
            supplier.TotalDue += pOrder.PaymentDue;
        }

        foreach (var detail in pOrder.POrderDetails)
        {
            foreach (var serial in detail.POProductDetails)
            {
                if (serial.ReturnSDetailID.HasValue && serial.ReturnSDetailID.Value > 0)
                {
                    var stockDetail = await _context.StockDetails
                        .FirstOrDefaultAsync(sd => sd.SDetailID == serial.ReturnSDetailID.Value, cancellationToken);

                    if (stockDetail != null)
                    {
                        stockDetail.Status = (int)EnumStockStatus.Stock; // restore to active stock
                    }
                }
            }

            var parentStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductID == detail.ProductID &&
                                          s.GodownID == detail.GodownID &&
                                          (detail.ColorID == null || s.ColorID == detail.ColorID) &&
                                          s.ConcernID == pOrder.ConcernID, cancellationToken);

            if (parentStock != null)
            {
                parentStock.Quantity += detail.Quantity;
            }

            _context.POProductDetails.RemoveRange(detail.POProductDetails);
        }

        _context.POrderDetails.RemoveRange(pOrder.POrderDetails);
        _context.POrders.Remove(pOrder);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
