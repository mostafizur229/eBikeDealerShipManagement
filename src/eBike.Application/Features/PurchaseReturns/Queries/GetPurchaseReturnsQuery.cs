using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseReturns.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseReturns.Queries;

public record GetPurchaseReturnsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? SupplierId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<PurchaseReturnListDto>>;

public class GetPurchaseReturnsQueryHandler : IRequestHandler<GetPurchaseReturnsQuery, List<PurchaseReturnListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseReturnsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PurchaseReturnListDto>> Handle(GetPurchaseReturnsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.POrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.POrderDetails)
            .Where(p => p.POType == 2 || p.Status == (int)EnumPurchaseType.ProductReturn || p.Status == (int)EnumPurchaseType.Return || p.ChallanNo.StartsWith("RC-"));

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(p => p.ConcernID == targetConcernId.Value);
        }

        if (request.SupplierId.HasValue && request.SupplierId.Value > 0)
        {
            query = query.Where(p => p.SupplierID == request.SupplierId.Value);
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
            .Select(p => new PurchaseReturnListDto
            {
                POrderID = p.POrderID,
                OrderDate = p.OrderDate,
                ChallanNo = p.ChallanNo,
                SupplierID = p.SupplierID,
                SupplierCode = p.Supplier != null ? p.Supplier.Code : null,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                SupplierContact = p.Supplier != null ? p.Supplier.ContactNo : null,
                GrandTotal = p.GrandTotal,
                TotalAmt = p.TotalAmt,
                RecAmt = p.RecAmt,
                PaymentDue = p.PaymentDue,
                TotalDue = p.TotalDue,
                Status = p.Status,
                StatusName = "Return",
                ItemCount = p.POrderDetails.Count,
                Remarks = p.Remarks
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}
