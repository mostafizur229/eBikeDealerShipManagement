using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseOrders.DTOs;
using eBike.Application.Features.PurchaseOrders.Queries;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseOrders.Commands;

public record UpdatePurchaseOrderCommand(
    int Id,
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
    List<UpdatePurchaseOrderDetailInput> Details,
    int? POType = null
) : IRequest<PurchaseOrderDto>;

public class UpdatePurchaseOrderDetailInput
{
    public int? POrderDetailId { get; set; }
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

public class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Purchase Order ID is required.");

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

        // Serials and duplicate Chassis No validations only apply to Vehicle Purchase Orders (POType != 2)
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

public class UpdatePurchaseOrderCommandHandler : IRequestHandler<UpdatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPurchaseOrderExecutionService _poService;
    private readonly IMediator _mediator;

    public UpdatePurchaseOrderCommandHandler(
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

    public async Task<PurchaseOrderDto> Handle(UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        // 1. Verify existing order
        var existingOrder = await _context.POrders
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.POProductDetails)
            .FirstOrDefaultAsync(p => p.POrderID == request.Id, cancellationToken);

        if (existingOrder == null)
            throw new NotFoundException($"Purchase Order with ID {request.Id} not found.");

        if (existingOrder.Status == 2)
            throw new BadRequestException("This Purchase Order has been returned/deleted and cannot be edited.");

        // 2. Check if any vehicle from this order was already sold
        int soldCount = await _poService.CheckProductStatusByPOIdAsync(request.Id, cancellationToken);
        if (soldCount > 0)
        {
            throw new BadRequestException("Some product(s) have already been sold from this order. This order cannot be edited.");
        }

        // 3. Verify Supplier
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierId, cancellationToken);
        if (supplier == null)
            throw new NotFoundException($"Supplier with ID {request.SupplierId} not found.");

        int poType = request.POType ?? existingOrder.POType;

        // 4. Duplicate Chassis validation: exclude existing chassis already in this order (only for Vehicle PO)
        if (poType != 2)
        {
            var existingChassisInThisOrder = existingOrder.POrderDetails
                .SelectMany(d => d.POProductDetails)
                .Select(s => s.IMENO.Trim().ToUpperInvariant())
                .ToHashSet();

            foreach (var detail in request.Details)
            {
                foreach (var serial in detail.Serials)
                {
                    var trimmed = serial.IMENO.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed) && trimmed != "No Barcode" && !existingChassisInThisOrder.Contains(trimmed.ToUpperInvariant()))
                    {
                        var existCount = await _poService.CheckIMENoDuplicacyByConcernIdAsync(targetConcernId, trimmed, cancellationToken);
                        if (existCount > 0)
                        {
                            throw new BadRequestException($"Chassis No '{trimmed}' is already registered in the system.");
                        }
                    }
                }
            }
        }

        // 5. Product codes lookup
        var productIds = request.Details.Select(d => d.ProductId).Distinct().ToList();
        var productsDict = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.ProductID))
            .ToDictionaryAsync(p => p.ProductID, p => p.Code, cancellationToken);

        // 6. Calculate financial amounts
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

        // 7. Map details and detect additions, updates, deletions
        var existingDetailsDict = existingOrder.POrderDetails.ToDictionary(d => d.POrderDetailID);
        var executionItems = new List<PurchaseOrderDetailExecutionItem>();

        // Process submitted items
        var requestDetailIds = new HashSet<int>();
        foreach (var d in request.Details)
        {
            int status = 3; // New by default
            decimal stockDiff = d.Quantity;

            if (d.POrderDetailId.HasValue && d.POrderDetailId.Value > 0 && existingDetailsDict.TryGetValue(d.POrderDetailId.Value, out var oldDetail))
            {
                status = 4; // Updated
                stockDiff = d.Quantity - oldDetail.Quantity;
                requestDetailIds.Add(d.POrderDetailId.Value);
            }

            executionItems.Add(new PurchaseOrderDetailExecutionItem
            {
                POrderDetailId = d.POrderDetailId,
                ProductId = d.ProductId,
                ProductCode = productsDict.TryGetValue(d.ProductId, out var code) ? code : d.ProductId.ToString(),
                ColorId = d.ColorId,
                Status = status,
                Quantity = d.Quantity,
                StockQtyDiff = stockDiff,
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
            });
        }

        // Process deleted items (in existingOrder but not in request)
        foreach (var oldDetail in existingOrder.POrderDetails)
        {
            if (!requestDetailIds.Contains(oldDetail.POrderDetailID))
            {
                executionItems.Add(new PurchaseOrderDetailExecutionItem
                {
                    POrderDetailId = oldDetail.POrderDetailID,
                    ProductId = oldDetail.ProductID,
                    ProductCode = oldDetail.ProductID.ToString(),
                    ColorId = oldDetail.ColorID,
                    Status = 5, // Deleted
                    Quantity = oldDetail.Quantity,
                    StockQtyDiff = oldDetail.Quantity,
                    UnitPrice = oldDetail.UnitPrice,
                    TAmount = oldDetail.TAmount,
                    GodownId = oldDetail.GodownID,
                    Serials = new List<POProductDetailExecutionItem>()
                });
            }
        }

        var execModel = new PurchaseOrderExecutionModel
        {
            OrderDate = request.OrderDate ?? existingOrder.OrderDate,
            ChallanNo = string.IsNullOrWhiteSpace(request.ChallanNo) ? existingOrder.ChallanNo : request.ChallanNo.Trim(),
            SupplierId = request.SupplierId,
            GrandTotal = grandTotal,
            TDiscount = tDiscount,
            TotalAmt = totalAmt,
            RecAmt = recAmt,
            PaymentDue = paymentDue,
            TotalDue = totalDue,
            AdjAmount = adjAmount,
            Status = existingOrder.Status,
            PPTDisAmt = pptDisAmt,
            NetDiscount = netDiscount,
            LabourCost = labourCost,
            ConcernId = targetConcernId,
            CreateDate = DateTime.Now,
            CreatedBy = userId,
            IsDamageOrder = request.IsDamageOrder ?? existingOrder.IsDamageOrder ?? 0,
            Remarks = request.Remarks ?? existingOrder.Remarks,
            TotalMushok = totalMushok,
            TotalTax = totalTax,
            POType = poType,
            Details = executionItems
        };

        // 8. Execute Update SP atomically
        bool success = await _poService.UpdatePurchaseOrderAsync(request.Id, execModel, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Failed to update Purchase Order in database.");
        }

        var resultDto = await _mediator.Send(new GetPurchaseOrderByIdQuery(request.Id), cancellationToken);
        return resultDto!;
    }
}
