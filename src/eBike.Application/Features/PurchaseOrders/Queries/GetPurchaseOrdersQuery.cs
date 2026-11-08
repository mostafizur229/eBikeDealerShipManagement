using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseOrders.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrdersQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? SupplierId = null,
    int? Status = null,
    string? Search = null,
    int? ConcernId = null,
    int? POType = 1
) : IRequest<List<PurchaseOrderListDto>>;

public class GetPurchaseOrdersQueryHandler : IRequestHandler<GetPurchaseOrdersQuery, List<PurchaseOrderListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseOrdersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PurchaseOrderListDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.POrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.POrderDetails)
            .AsQueryable();

        if (request.POType.HasValue && request.POType.Value > 0)
        {
            if (request.POType.Value == 2)
            {
                query = query.Where(p => p.POType == 2 || p.Status == (int)EnumPurchaseType.ProductReturn || p.Status == (int)EnumPurchaseType.Return || p.ChallanNo.StartsWith("RC-"));
            }
            else
            {
                query = query.Where(p => p.POType == request.POType.Value && p.Status != (int)EnumPurchaseType.ProductReturn && p.Status != (int)EnumPurchaseType.Return && !p.ChallanNo.StartsWith("RC-"));
            }
        }

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(p => p.ConcernID == targetConcernId.Value);
        }

        if (request.SupplierId.HasValue && request.SupplierId.Value > 0)
        {
            query = query.Where(p => p.SupplierID == request.SupplierId.Value);
        }

        if (request.Status.HasValue && request.Status.Value > 0)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(p => p.OrderDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.OrderDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.ChallanNo.ToLower().Contains(search) ||
                (p.Remarks != null && p.Remarks.ToLower().Contains(search)) ||
                (p.Supplier != null && (
                    (p.Supplier.Name != null && p.Supplier.Name.ToLower().Contains(search)) ||
                    (p.Supplier.Code != null && p.Supplier.Code.ToLower().Contains(search)) ||
                    (p.Supplier.ContactNo != null && p.Supplier.ContactNo.ToLower().Contains(search))
                )));
        }

        var orders = await query
            .OrderByDescending(p => p.OrderDate)
            .ThenByDescending(p => p.POrderID)
            .Select(p => new
            {
                p.POrderID,
                p.OrderDate,
                p.ChallanNo,
                p.SupplierID,
                SupplierCode = p.Supplier != null ? p.Supplier.Code : null,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                p.GrandTotal,
                p.TDiscount,
                p.TotalAmt,
                p.RecAmt,
                p.PaymentDue,
                p.TotalDue,
                p.Status,
                p.PPDisAmt,
                p.NetDiscount,
                p.LaborCost,
                p.AdjAmount,
                p.ConcernID,
                p.IsDamageOrder,
                p.Remarks,
                TotalItemsCount = p.POrderDetails.Count,
                TotalQuantity = p.POrderDetails.Sum(d => d.Quantity)
            })
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.POrderID).ToList();

        // Check sold status in bulk for the returned orders
        var soldOrderIds = await _context.StockDetails
            .AsNoTracking()
            .Where(sd => sd.Status == (int)EnumStockStatus.Sold &&
                         _context.POrderDetails.Any(pod => pod.POrderDetailID == sd.POrderDetailID && orderIds.Contains(pod.POrderID)))
            .Select(sd => sd.POrderDetail != null ? sd.POrderDetail.POrderID : 0)
            .Distinct()
            .ToListAsync(cancellationToken);

        var soldSet = new HashSet<int>(soldOrderIds);

        return orders.Select(p => new PurchaseOrderListDto
        {
            POrderID = p.POrderID,
            OrderDate = p.OrderDate,
            ChallanNo = p.ChallanNo,
            SupplierID = p.SupplierID,
            SupplierCode = p.SupplierCode,
            SupplierName = p.SupplierName,
            GrandTotal = p.GrandTotal,
            TDiscount = p.TDiscount,
            TotalAmt = p.TotalAmt,
            RecAmt = p.RecAmt,
            PaymentDue = p.PaymentDue,
            TotalDue = p.TotalDue,
            Status = p.Status,
            PPDisAmt = p.PPDisAmt,
            NetDiscount = p.NetDiscount,
            LaborCost = p.LaborCost,
            AdjAmount = p.AdjAmount,
            ConcernID = p.ConcernID,
            IsDamageOrder = p.IsDamageOrder,
            Remarks = p.Remarks,
            TotalItemsCount = p.TotalItemsCount,
            TotalQuantity = p.TotalQuantity,
            HasSoldProducts = soldSet.Contains(p.POrderID)
        }).ToList();
    }
}
