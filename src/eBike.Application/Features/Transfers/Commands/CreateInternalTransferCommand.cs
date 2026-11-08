using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Commands;

public record CreateInternalTransferCommand(
    DateTime? TransferDate,
    int FromGodownId,
    int ToGodownId,
    int ProductId,
    int StockDetailId,
    decimal Quantity,
    int? ConcernId
) : IRequest<bool>;

public class CreateInternalTransferCommandValidator : AbstractValidator<CreateInternalTransferCommand>
{
    public CreateInternalTransferCommandValidator()
    {
        RuleFor(x => x.FromGodownId).GreaterThan(0).WithMessage("From Godown is required.");
        RuleFor(x => x.ToGodownId).GreaterThan(0).WithMessage("To Godown is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Product is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x).Must(x => x.FromGodownId != x.ToGodownId).WithMessage("From Godown and To Godown cannot be the same.");
    }
}

public class CreateInternalTransferCommandHandler : IRequestHandler<CreateInternalTransferCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CreateInternalTransferCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(CreateInternalTransferCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var stockDetail = await _context.StockDetails
            .Include(sd => sd.Stock)
            .FirstOrDefaultAsync(sd => sd.SDetailID == request.StockDetailId, cancellationToken);

        if (stockDetail == null || stockDetail.GodownID != request.FromGodownId || stockDetail.Quantity < request.Quantity)
        {
            throw new BadRequestException("Selected vehicle or item is not available in the source godown with sufficient quantity.");
        }

        // 1. Decrement source stock
        if (stockDetail.Stock != null && stockDetail.Stock.Quantity > 0)
        {
            stockDetail.Stock.Quantity -= request.Quantity;
        }

        // 2. Find or create destination stock in ToGodown
        var toStock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ConcernID == targetConcernId &&
                                      s.ProductID == request.ProductId &&
                                      s.GodownID == request.ToGodownId &&
                                      s.ColorID == stockDetail.ColorID,
                                 cancellationToken);

        if (toStock == null)
        {
            toStock = new Stock
            {
                ProductID = request.ProductId,
                GodownID = request.ToGodownId,
                ColorID = stockDetail.ColorID ?? 0,
                Quantity = request.Quantity,
                MRPPrice = stockDetail.PRate,
                LPPrice = stockDetail.PRate,
                ConcernID = targetConcernId,
                EntryDate = DateTime.Now,
                CreatedBy = userId,
                CreateDate = DateTime.Now
            };
            _context.Stocks.Add(toStock);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            toStock.Quantity += request.Quantity;
        }

        // 3. Update stock detail godown
        stockDetail.GodownID = request.ToGodownId;
        stockDetail.StockID = toStock.StockID;

        // 4. Record TransferHistory
        var history = new TransferHistory
        {
            TransferDate = request.TransferDate ?? DateTime.Today,
            FromGodown = request.FromGodownId,
            ToGodown = request.ToGodownId,
            ProductId = request.ProductId,
            Qty = request.Quantity,
            FromSDetailID = stockDetail.SDetailID,
            ToSDetailID = stockDetail.SDetailID,
            ConcernID = targetConcernId,
            CreatedBy = userId
        };
        _context.TransferHistories.Add(history);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
