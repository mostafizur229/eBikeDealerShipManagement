using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using eBike.Application.Features.Transfers.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Commands;

public record CreateTransferCommand(
    string? TransferNo,
    DateTime? TransferDate,
    int ToConcernId,
    string? Remarks,
    int? ConcernId,
    List<CreateTransferItemInput> Items
) : IRequest<TransferDto>;

public class CreateTransferItemInput
{
    public int ProductId { get; set; }
    public int StockDetailId { get; set; }
    public int ToGodownId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal PRate { get; set; }
}

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.ToConcernId)
            .GreaterThan(0).WithMessage("Destination concern/branch is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one vehicle/product must be added for transfer.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.StockDetailId)
                .GreaterThan(0).WithMessage("Stock item is required.");

            item.RuleFor(i => i.ToGodownId)
                .GreaterThan(0).WithMessage("Destination godown is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Transfer quantity must be greater than zero.");
        });
    }
}

public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, TransferDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public CreateTransferCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<TransferDto> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        int sourceConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        if (sourceConcernId == request.ToConcernId)
        {
            throw new BadRequestException("Source concern and destination concern cannot be the same. For internal godown movements, use Internal Transfer.");
        }

        var toConcern = await _context.SisterConcerns
            .FirstOrDefaultAsync(c => c.ConcernID == request.ToConcernId, cancellationToken);
        if (toConcern == null)
            throw new NotFoundException($"Destination Sister Concern with ID {request.ToConcernId} not found.");

        string transferNo = string.IsNullOrWhiteSpace(request.TransferNo)
            ? await _mediator.Send(new GetNextTransferNoQuery(sourceConcernId), cancellationToken)
            : request.TransferNo.Trim();

        decimal totalAmount = request.Items.Sum(i => i.PRate * i.Quantity);

        var transfer = new Transfer
        {
            TransferNo = transferNo,
            TransferDate = request.TransferDate ?? DateTime.Today,
            FromConcernID = sourceConcernId,
            ToConcernID = request.ToConcernId,
            TotalAmount = totalAmount,
            Remarks = request.Remarks,
            Status = 1, // Transferred
            ConcernID = sourceConcernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now
        };

        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            var stockDetail = await _context.StockDetails
                .Include(sd => sd.Stock)
                .Include(sd => sd.Product)
                .Include(sd => sd.Color)
                .FirstOrDefaultAsync(sd => sd.SDetailID == item.StockDetailId, cancellationToken);

            if (stockDetail == null || stockDetail.Quantity < item.Quantity)
            {
                throw new BadRequestException($"Selected vehicle/stock item #{item.StockDetailId} is not available in sufficient quantity.");
            }

            // 1. Decrement source stock
            if (stockDetail.Stock != null && stockDetail.Stock.Quantity > 0)
            {
                stockDetail.Stock.Quantity -= item.Quantity;
            }

            // 2. Find target product in destination concern by code/name
            var targetProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ConcernID == request.ToConcernId &&
                                          (p.Code == stockDetail.Product!.Code || p.ProductName == stockDetail.Product.ProductName),
                                     cancellationToken)
                                ?? stockDetail.Product!;

            // 3. Find target color in destination concern
            Color? targetColor = null;
            if (stockDetail.Color != null)
            {
                targetColor = await _context.Colors
                    .FirstOrDefaultAsync(c => c.ConcernID == request.ToConcernId && c.Name == stockDetail.Color.Name, cancellationToken)
                    ?? stockDetail.Color;
            }

            int toColorId = targetColor?.ColorID ?? stockDetail.ColorID ?? 0;

            // 4. Find or create destination Stock
            var targetStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ConcernID == request.ToConcernId &&
                                          s.ProductID == targetProduct.ProductID &&
                                          s.GodownID == item.ToGodownId &&
                                          s.ColorID == toColorId,
                                     cancellationToken);

            if (targetStock == null)
            {
                targetStock = new Stock
                {
                    ProductID = targetProduct.ProductID,
                    GodownID = item.ToGodownId,
                    ColorID = toColorId,
                    Quantity = item.Quantity,
                    MRPPrice = item.PRate,
                    LPPrice = item.PRate,
                    ConcernID = request.ToConcernId,
                    EntryDate = DateTime.Now,
                    CreatedBy = userId,
                    CreateDate = DateTime.Now
                };
                _context.Stocks.Add(targetStock);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                targetStock.Quantity += item.Quantity;
            }

            // 5. Create TransferDetail
            var detail = new TransferDetail
            {
                TransferID = transfer.TransferID,
                ProductID = stockDetail.ProductID,
                ToProductID = targetProduct.ProductID,
                ToGodownID = item.ToGodownId,
                ToColorID = toColorId,
                PRate = item.PRate,
                Quantity = item.Quantity,
                UTAmount = item.PRate * item.Quantity,
                SDetailID = stockDetail.SDetailID,
                IMEI = stockDetail.IMENO
            };
            _context.TransferDetails.Add(detail);
            await _context.SaveChangesAsync(cancellationToken);

            // 6. Update StockDetail location and linkage
            stockDetail.GodownID = item.ToGodownId;
            stockDetail.ProductID = targetProduct.ProductID;
            stockDetail.ColorID = toColorId;
            stockDetail.StockID = targetStock.StockID;
            stockDetail.TransferDetailID = detail.TDetailID;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetTransferByIdQuery(transfer.TransferID), cancellationToken);
    }
}
