using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseReturns.DTOs;
using eBike.Application.Features.PurchaseReturns.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseReturns.Commands;

public record CreatePurchaseReturnCommand(
    DateTime? ReturnDate,
    string? ChallanNo,
    int SupplierId,
    decimal? RecAmt,
    string? Remarks,
    int? ConcernId,
    List<CreatePurchaseReturnItemInput> Items
) : IRequest<PurchaseReturnDto>;

public class CreatePurchaseReturnItemInput
{
    public int ProductId { get; set; }
    public int? StockDetailId { get; set; }
    public int? ColorId { get; set; }
    public int GodownId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmt { get; set; }
    public string? IMENO { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
}

public class CreatePurchaseReturnCommandValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be returned.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Product is required.");

            item.RuleFor(i => i.GodownId)
                .GreaterThan(0).WithMessage("Godown is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Return quantity must be greater than zero.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit rate cannot be negative.");
        });
    }
}

public class CreatePurchaseReturnCommandHandler : IRequestHandler<CreatePurchaseReturnCommand, PurchaseReturnDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPurchaseOrderExecutionService _poService;
    private readonly IMediator _mediator;

    public CreatePurchaseReturnCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPurchaseOrderExecutionService poService,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _poService = poService;
        _mediator = mediator;
    }

    public async Task<PurchaseReturnDto> Handle(CreatePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierId, cancellationToken);
        if (supplier == null)
            throw new NotFoundException($"Supplier with ID {request.SupplierId} not found.");

        string challanNo = string.IsNullOrWhiteSpace(request.ChallanNo)
            ? await _poService.GetNextChallanNoAsync(targetConcernId, 2, cancellationToken)
            : request.ChallanNo.Trim();

        decimal grandTotal = request.Items.Sum(i => i.TotalAmt > 0 ? i.TotalAmt : (i.Quantity * i.UnitPrice));
        decimal recAmt = request.RecAmt ?? 0;
        decimal paymentDue = grandTotal - recAmt;
        decimal totalDue = supplier.TotalDue - paymentDue;

        var pOrder = new POrder
        {
            OrderDate = request.ReturnDate ?? DateTime.Today,
            ChallanNo = challanNo,
            SupplierID = request.SupplierId,
            GrandTotal = grandTotal,
            TDiscount = 0,
            TotalAmt = grandTotal,
            RecAmt = recAmt,
            PaymentDue = paymentDue,
            TotalDue = totalDue,
            Status = (int)EnumPurchaseType.ProductReturn,
            POType = 2,
            ConcernID = targetConcernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now,
            Remarks = request.Remarks,
            TotalMushok = 0,
            TotalTax = 0,
            PPDisAmt = 0,
            NetDiscount = 0,
            LaborCost = 0,
            AdjAmount = 0
        };

        _context.POrders.Add(pOrder);
        await _context.SaveChangesAsync(cancellationToken);

        supplier.TotalDue = totalDue;

        foreach (var item in request.Items)
        {
            var pOrderDetail = new POrderDetail
            {
                POrderID = pOrder.POrderID,
                ProductID = item.ProductId,
                ColorID = item.ColorId,
                GodownID = item.GodownId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TAmount = item.TotalAmt > 0 ? item.TotalAmt : (item.Quantity * item.UnitPrice),
                MRPRate = item.UnitPrice,
                DPRate = item.UnitPrice,
                SalesRate = item.UnitPrice
            };

            _context.POrderDetails.Add(pOrderDetail);
            await _context.SaveChangesAsync(cancellationToken);

            if (item.StockDetailId.HasValue && item.StockDetailId.Value > 0)
            {
                var stockDetail = await _context.StockDetails
                    .FirstOrDefaultAsync(sd => sd.SDetailID == item.StockDetailId.Value, cancellationToken);

                if (stockDetail != null)
                {
                    stockDetail.Status = (int)EnumStockStatus.Return;
                    var poProductDetail = new POProductDetail
                    {
                        POrderDetailID = pOrderDetail.POrderDetailID,
                        ProductID = item.ProductId,
                        ColorID = item.ColorId,
                        IMENO = !string.IsNullOrWhiteSpace(item.IMENO) ? item.IMENO : stockDetail.IMENO,
                        EngineNo = !string.IsNullOrWhiteSpace(item.EngineNo) ? item.EngineNo : stockDetail.EngineNo,
                        KeyNo = !string.IsNullOrWhiteSpace(item.KeyNo) ? item.KeyNo : stockDetail.KeyNo,
                        ReturnSDetailID = item.StockDetailId.Value,
                        ReturnQty = item.Quantity
                    };
                    _context.POProductDetails.Add(poProductDetail);
                }

                var parentStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductID == item.ProductId &&
                                              s.GodownID == item.GodownId &&
                                              (item.ColorId == null || s.ColorID == item.ColorId) &&
                                              s.ConcernID == targetConcernId, cancellationToken);

                if (parentStock != null)
                {
                    parentStock.Quantity = Math.Max(0, parentStock.Quantity - item.Quantity);
                    parentStock.ModifiedDate = DateTime.Now;
                    parentStock.ModifiedBy = userId;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(item.IMENO))
                {
                    var poProductDetail = new POProductDetail
                    {
                        POrderDetailID = pOrderDetail.POrderDetailID,
                        ProductID = item.ProductId,
                        ColorID = item.ColorId,
                        IMENO = item.IMENO,
                        EngineNo = item.EngineNo ?? string.Empty,
                        KeyNo = item.KeyNo ?? string.Empty,
                        ReturnQty = item.Quantity
                    };
                    _context.POProductDetails.Add(poProductDetail);
                }

                var parentStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductID == item.ProductId &&
                                              s.GodownID == item.GodownId &&
                                              s.ConcernID == targetConcernId, cancellationToken);

                if (parentStock != null)
                {
                    parentStock.Quantity = Math.Max(0, parentStock.Quantity - item.Quantity);
                    parentStock.ModifiedDate = DateTime.Now;
                    parentStock.ModifiedBy = userId;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = await _mediator.Send(new GetPurchaseReturnByIdQuery(pOrder.POrderID), cancellationToken);
        return resultDto!;
    }
}
