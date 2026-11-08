using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Commands;

public record DeleteTransferCommand(int Id) : IRequest<bool>;

public class DeleteTransferCommandHandler : IRequestHandler<DeleteTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTransferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _context.Transfers
            .Include(t => t.TransferDetails)
            .FirstOrDefaultAsync(t => t.TransferID == request.Id, cancellationToken);

        if (transfer == null)
            throw new NotFoundException($"Transfer order with ID {request.Id} was not found.");

        foreach (var detail in transfer.TransferDetails)
        {
            var stockDetail = await _context.StockDetails
                .Include(sd => sd.Stock)
                .FirstOrDefaultAsync(sd => sd.SDetailID == detail.SDetailID, cancellationToken);

            if (stockDetail != null)
            {
                // Decrement destination stock
                if (stockDetail.Stock != null && stockDetail.Stock.Quantity >= detail.Quantity)
                {
                    stockDetail.Stock.Quantity -= detail.Quantity;
                }

                // Restore source product & reset transfer link
                stockDetail.TransferDetailID = 0;
                stockDetail.ProductID = detail.ProductID;

                // Re-increment source stock
                var sourceStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ConcernID == transfer.FromConcernID &&
                                              s.ProductID == detail.ProductID, cancellationToken);
                if (sourceStock != null)
                {
                    sourceStock.Quantity += detail.Quantity;
                    stockDetail.StockID = sourceStock.StockID;
                }
            }
        }

        _context.TransferDetails.RemoveRange(transfer.TransferDetails);
        _context.Transfers.Remove(transfer);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
