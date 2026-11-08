using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseOrders.DTOs;
using eBike.Application.Features.PurchaseOrders.Queries;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseOrders.Commands;

public record CreatePurchaseOrderCommand(
    DateTime? OrderDate,
    string? ChallanNo,
    int SupplierId,
    decimal GrandTotal,
    decimal? TDiscount,
    decimal TotalAmt,
    decimal? RecAmt,
    decimal? PaymentDue,
    decimal? AdjAmount,
    decimal? PPTDisAmt,
    decimal? NetDiscount,
    decimal? LabourCost,
    decimal? TotalMushok,
    decimal? TotalTax,
    string? Remarks,
    int? IsDamageOrder,
    int? ConcernId,
    List<CreatePurchaseOrderDetailInput> Details,
    int POType = 1
) : IRequest<PurchaseOrderDto>;

public class CreatePurchaseOrderDetailInput
{
    public int ProductId { get; set; }
    public int? ColorId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TAmount { get; set; }
    public decimal? PPDisPer { get; set; }
    public decimal? PPDisAmt { get; set; }
    public decimal? MrpRate { get; set; }
    public decimal? SalesRate { get; set; }
    public decimal? ExtraPPDISPer { get; set; }
    public decimal? ExtraPPDISAmt { get; set; }
    public decimal? PPOffer { get; set; }
    public decimal? CreditSalesRate { get; set; }
    public decimal? CRSalesRate12Month { get; set; }
    public decimal? CRSalesRate3Month { get; set; }
    public int GodownId { get; set; }
    public decimal? Mushok { get; set; }
    public decimal? MRPWMushok { get; set; }
    public decimal? DPRate { get; set; }
    public decimal? PWTaxAmt { get; set; }
    public List<CreatePOProductDetailInput> Serials { get; set; } = new();
}

public class CreatePOProductDetailInput
{
    public string IMENO { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public int? DamagePOPDID { get; set; }
}

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one purchase order item is required.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .GreaterThan(0).WithMessage("Product is required.");

            detail.RuleFor(d => d.GodownId)
                .GreaterThan(0).WithMessage("Godown/Warehouse is required.");

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            detail.RuleFor(d => d.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        });

        When(x => x.POType != 2, () =>
        {
            RuleForEach(x => x.Details).ChildRules(detail =>
            {
                detail.RuleFor(d => d.Serials)
                    .Must((d, serials) => serials != null && serials.Count == (int)d.Quantity)
                    .WithMessage("Number of serial entries (Chassis/Engine) must match the item Quantity for vehicle orders.");

                detail.RuleForEach(d => d.Serials).ChildRules(serial =>
                {
                    serial.RuleFor(s => s.IMENO)
                        .NotEmpty().WithMessage("Chassis number / IMEI is required for each vehicle.");

                    serial.RuleFor(s => s.EngineNo)
                        .NotEmpty().WithMessage("Engine number is required for each vehicle.");
                });
            });

            RuleFor(x => x.Details)
                .Must(details =>
                {
                    var allImeis = details.SelectMany(d => d.Serials)
                        .Select(s => s.IMENO.Trim().ToUpperInvariant())
                        .Where(s => !string.IsNullOrEmpty(s) && s != "NO BARCODE")
                        .ToList();
                    return allImeis.Count == allImeis.Distinct().Count();
                })
                .WithMessage("Duplicate Chassis numbers found in the order items.");
        });
    }
}

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPurchaseOrderExecutionService _poService;
    private readonly IMediator _mediator;

    public CreatePurchaseOrderCommandHandler(
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

    public async Task<PurchaseOrderDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierId, cancellationToken);
        if (supplier == null)
            throw new NotFoundException($"Supplier with ID {request.SupplierId} not found.");

        if (request.POType != 2)
        {
            foreach (var detail in request.Details)
            {
                foreach (var serial in detail.Serials)
                {
                    if (!string.IsNullOrWhiteSpace(serial.IMENO) && serial.IMENO != "No Barcode")
                    {
                        var existCount = await _poService.CheckIMENoDuplicacyByConcernIdAsync(targetConcernId, serial.IMENO.Trim(), cancellationToken);
                        if (existCount > 0)
                        {
                            throw new BadRequestException($"Chassis No '{serial.IMENO.Trim()}' is already registered in the system.");
                        }
                    }
                }
            }
        }

        string challanNo = string.IsNullOrWhiteSpace(request.ChallanNo)
            ? await _poService.GetNextChallanNoAsync(targetConcernId, request.POType, cancellationToken)
            : request.ChallanNo.Trim();

        var productIds = request.Details.Select(d => d.ProductId).Distinct().ToList();
        var productsDict = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductID))
            .ToDictionaryAsync(p => p.ProductID, p => p.Code, cancellationToken);

        decimal grandTotal = request.GrandTotal > 0 ? request.GrandTotal : request.Details.Sum(d => d.TAmount);
        decimal tDiscount = request.TDiscount ?? 0;
        decimal adjAmount = request.AdjAmount ?? 0;
        decimal labourCost = request.LabourCost ?? 0;
        decimal totalMushok = request.TotalMushok ?? request.Details.Sum(d => d.Mushok ?? 0);
        decimal totalTax = request.TotalTax ?? request.Details.Sum(d => d.PWTaxAmt ?? 0);
        decimal pptDisAmt = request.PPTDisAmt ?? request.Details.Sum(d => d.PPDisAmt ?? 0);
        decimal netDiscount = request.NetDiscount ?? (pptDisAmt + tDiscount);
        decimal totalAmt = request.TotalAmt > 0 ? request.TotalAmt : (grandTotal - tDiscount + adjAmount + labourCost + totalTax);
        decimal recAmt = request.RecAmt ?? 0;
        decimal paymentDue = request.PaymentDue ?? (totalAmt - recAmt);
        decimal totalDue = supplier.TotalDue + paymentDue;

        var execModel = new PurchaseOrderExecutionModel
        {
            OrderDate = request.OrderDate ?? DateTime.Now,
            ChallanNo = challanNo,
            SupplierId = request.SupplierId,
            GrandTotal = grandTotal,
            TDiscount = tDiscount,
            TotalAmt = totalAmt,
            RecAmt = recAmt,
            PaymentDue = paymentDue,
            TotalDue = totalDue,
            AdjAmount = adjAmount,
            Status = (int)EnumPurchaseType.Purchase, // 1
            PPTDisAmt = pptDisAmt,
            NetDiscount = netDiscount,
            LabourCost = labourCost,
            ConcernId = targetConcernId,
            CreateDate = DateTime.Now,
            CreatedBy = userId,
            IsDamageOrder = request.IsDamageOrder ?? 0,
            Remarks = request.Remarks,
            TotalMushok = totalMushok,
            TotalTax = totalTax,
            POType = request.POType,
            Details = request.Details.Select(d => new PurchaseOrderDetailExecutionItem
            {
                POrderDetailId = null,
                ProductId = d.ProductId,
                ProductCode = productsDict.TryGetValue(d.ProductId, out var code) ? code : d.ProductId.ToString(),
                ColorId = d.ColorId,
                Status = (int)EnumPurchaseType.Purchase,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TAmount = d.TAmount > 0 ? d.TAmount : (d.Quantity * d.UnitPrice),
                PPDisPer = d.PPDisPer ?? 0,
                PPDisAmt = d.PPDisAmt ?? 0,
                MrpRate = d.MrpRate ?? 0,
                SalesRate = d.SalesRate ?? 0,
                ExtraPPDISPer = d.ExtraPPDISPer ?? 0,
                ExtraPPDISAmt = d.ExtraPPDISAmt ?? 0,
                PPOffer = d.PPOffer ?? 0,
                CreditSalesRate = d.CreditSalesRate ?? 0,
                CRSalesRate12Month = d.CRSalesRate12Month ?? 0,
                CRSalesRate3Month = d.CRSalesRate3Month ?? 0,
                GodownId = d.GodownId,
                Mushok = d.Mushok ?? 0,
                MRPWMushok = d.MRPWMushok ?? 0,
                DPRate = d.DPRate ?? d.UnitPrice,
                RecQuantity = 0,
                DOrderDetailId = 0,
                PWTaxAmt = d.PWTaxAmt ?? 0,
                Serials = d.Serials.Select(s => new POProductDetailExecutionItem
                {
                    ProductId = d.ProductId,
                    ColorId = d.ColorId,
                    IMENO = s.IMENO.Trim(),
                    EngineNo = s.EngineNo.Trim(),
                    KeyNo = s.KeyNo.Trim(),
                    DamagePOPDID = s.DamagePOPDID ?? 0
                }).ToList()
            }).ToList()
        };

        bool success = await _poService.AddPurchaseOrderAsync(execModel, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Failed to save Purchase Order in the database.");
        }

        var createdOrder = await _context.POrders
            .AsNoTracking()
            .Where(p => p.ChallanNo == challanNo && p.ConcernID == targetConcernId)
            .OrderByDescending(p => p.POrderID)
            .FirstOrDefaultAsync(cancellationToken);

        if (createdOrder == null)
        {
            throw new InvalidOperationException("Purchase Order was created but could not be retrieved.");
        }

        var resultDto = await _mediator.Send(new GetPurchaseOrderByIdQuery(createdOrder.POrderID), cancellationToken);
        return resultDto!;
    }
}
