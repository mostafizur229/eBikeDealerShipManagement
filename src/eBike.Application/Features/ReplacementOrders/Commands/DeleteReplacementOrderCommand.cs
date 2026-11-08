using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Commands;

public record DeleteReplacementOrderCommand(int Id) : IRequest<bool>;

public class DeleteReplacementOrderCommandHandler : IRequestHandler<DeleteReplacementOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteReplacementOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteReplacementOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SOrders
            .Include(s => s.SOrderDetails)
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.SOrderID == request.Id && s.IsReplacement == 1, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Replacement order with ID {request.Id} was not found.");

        // 1. Revert new replacement items
        foreach (var newItem in order.SOrderDetails)
        {
            var stockDetail = await _context.StockDetails
                .Include(sd => sd.Stock)
                .FirstOrDefaultAsync(sd => sd.SDetailID == newItem.SDetailID, cancellationToken);

            if (stockDetail != null)
            {
                stockDetail.Status = (int)EnumStockStatus.Stock;
                if (stockDetail.Stock != null)
                {
                    stockDetail.Stock.Quantity += newItem.Quantity;
                }
            }
        }

        // 2. Revert original damaged items linked via RepOrderID
        var originalItems = await _context.SOrderDetails
            .Where(d => d.RepOrderID == order.SOrderID)
            .ToListAsync(cancellationToken);

        foreach (var origItem in originalItems)
        {
            var origStockDetail = await _context.StockDetails
                .Include(sd => sd.Stock)
                .FirstOrDefaultAsync(sd => sd.SDetailID == origItem.SDetailID, cancellationToken);

            if (origStockDetail != null)
            {
                origStockDetail.Status = (int)EnumStockStatus.Sold;
                if (origItem.ReplaceProductType == 2 && origStockDetail.Stock != null && origStockDetail.Stock.Quantity > 0)
                {
                    origStockDetail.Stock.Quantity -= 1;
                }
            }

            origItem.RepOrderID = null;
            origItem.RStockDetailId = null;
            origItem.RepUnitPrice = null;
        }

        // 3. Revert customer due balance
        if (order.Customer != null)
        {
            order.Customer.TotalDue -= order.PaymentDue;
        }

        // 4. Remove order and details
        _context.SOrderDetails.RemoveRange(order.SOrderDetails);
        _context.SOrders.Remove(order);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
