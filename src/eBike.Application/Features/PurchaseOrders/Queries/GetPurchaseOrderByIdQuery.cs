using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseOrders.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrderByIdQuery(int Id) : IRequest<PurchaseOrderDto?>;

public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseOrderDto?> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.POrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Product)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Color)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Godown)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.POProductDetails)
            .FirstOrDefaultAsync(p => p.POrderID == request.Id, cancellationToken);

        if (order == null)
            return null;

        var hasSoldProducts = await _context.StockDetails
            .AsNoTracking()
            .AnyAsync(sd => sd.Status == (int)EnumStockStatus.Sold &&
                            _context.POrderDetails.Any(pod => pod.POrderDetailID == sd.POrderDetailID && pod.POrderID == order.POrderID),
                      cancellationToken);

        var detailIds = order.POrderDetails.Select(d => d.POrderDetailID).ToList();
        var stockStatusDict = await _context.StockDetails
            .AsNoTracking()
            .Where(sd => detailIds.Contains(sd.POrderDetailID))
            .ToDictionaryAsync(sd => sd.IMENO, sd => sd.Status, cancellationToken);

        var dto = new PurchaseOrderDto
        {
            POrderID = order.POrderID,
            OrderDate = order.OrderDate,
            ChallanNo = order.ChallanNo,
            SupplierID = order.SupplierID,
            SupplierCode = order.Supplier?.Code,
            SupplierName = order.Supplier?.Name,
            SupplierContact = order.Supplier?.ContactNo,
            SupplierTotalDue = order.Supplier?.TotalDue ?? 0,
            GrandTotal = order.GrandTotal,
            TDiscount = order.TDiscount,
            TotalAmt = order.TotalAmt,
            RecAmt = order.RecAmt,
            PaymentDue = order.PaymentDue,
            TotalDue = order.TotalDue,
            Status = order.Status,
            StatusName = Enum.IsDefined(typeof(EnumPurchaseType), order.Status)
                ? ((EnumPurchaseType)order.Status).ToString()
                : order.Status.ToString(),
            PPDisAmt = order.PPDisAmt,
            NetDiscount = order.NetDiscount,
            LaborCost = order.LaborCost,
            AdjAmount = order.AdjAmount,
            ConcernID = order.ConcernID,
            IsDamageOrder = order.IsDamageOrder,
            Remarks = order.Remarks,
            TotalMushok = order.TotalMushok,
            TotalTax = order.TotalTax,
            POType = order.POType,
            HasSoldProducts = hasSoldProducts,
            Details = order.POrderDetails.Select(d => new PurchaseOrderDetailDto
            {
                POrderDetailID = d.POrderDetailID,
                ProductID = d.ProductID,
                ProductCode = d.Product?.Code,
                ProductName = d.Product?.ProductName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TAmount = d.TAmount,
                PPDISPer = d.PPDISPer,
                PPDISAmt = d.PPDISAmt,
                MRPRate = d.MRPRate,
                ColorID = d.ColorID,
                ColorName = d.Color?.Name,
                SalesRate = d.SalesRate,
                ExtraPPDISPer = d.ExtraPPDISPer,
                ExtraPPDISAmt = d.ExtraPPDISAmt,
                PPOFFER = d.PPOFFER,
                CreditSalesRate = d.CreditSalesRate,
                CRSalesRate12Month = d.CRSalesRate12Month,
                CRSalesRate3Month = d.CRSalesRate3Month,
                GodownID = d.GodownID,
                GodownName = d.Godown?.Name,
                Mushok = d.Mushok,
                MRPWMushok = d.MRPWMushok,
                DPRate = d.DPRate,
                RecQuantity = d.RecQuantity,
                DOrderDetailId = d.DOrderDetailId,
                PWTaxAmt = d.PWTaxAmt,
                Serials = d.POProductDetails.Select(s => new POProductDetailDto
                {
                    POPDID = s.POPDID,
                    ProductID = s.ProductID,
                    ColorID = s.ColorID,
                    ColorName = d.Color?.Name,
                    IMENO = s.IMENO,
                    EngineNo = s.EngineNo,
                    KeyNo = s.KeyNo,
                    DamagePOPDID = s.DamagePOPDID,
                    IsDamageReplaced = s.IsDamageReplaced,
                    StatusName = stockStatusDict.TryGetValue(s.IMENO, out var st)
                        ? ((EnumStockStatus)st).ToString()
                        : "Unknown"
                }).ToList()
            }).ToList()
        };

        return dto;
    }
}
