using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetSalesOrdersQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? CustomerId = null,
    int? Status = null,
    string? Search = null,
    int? ConcernId = null,
    int? SOType = 1
) : IRequest<List<SalesOrderListDto>>;

public class GetSalesOrdersQueryHandler : IRequestHandler<GetSalesOrdersQuery, List<SalesOrderListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetSalesOrdersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<SalesOrderListDto>> Handle(GetSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SOrderDetails)
            .AsQueryable();

        if (request.SOType.HasValue && request.SOType.Value > 0)
        {
            query = query.Where(s => s.SOType == request.SOType.Value);
        }

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(s => s.ConcernID == targetConcernId.Value);
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            query = query.Where(s => s.CustomerID == request.CustomerId.Value);
        }

        if (request.Status.HasValue && request.Status.Value > 0)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }
        else
        {
            query = query.Where(s => s.Status != (int)Domain.Enums.EnumSalesType.ProductReturn && s.Status != (int)Domain.Enums.EnumSalesType.Return && !s.InvoiceNo.StartsWith("SR-") && s.IsReplacement != 1);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(s => s.InvoiceDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(s => s.InvoiceDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s =>
                s.InvoiceNo.ToLower().Contains(search) ||
                (s.MRNo != null && s.MRNo.ToLower().Contains(search)) ||
                (s.LedgerNo != null && s.LedgerNo.ToLower().Contains(search)) ||
                (s.REGMRNo != null && s.REGMRNo.ToLower().Contains(search)) ||
                (s.Remarks != null && s.Remarks.ToLower().Contains(search)) ||
                (s.Customer != null && (
                    (s.Customer.Name != null && s.Customer.Name.ToLower().Contains(search)) ||
                    (s.Customer.Code != null && s.Customer.Code.ToLower().Contains(search)) ||
                    (s.Customer.ContactNo != null && s.Customer.ContactNo.ToLower().Contains(search))
                )));
        }

        return await query
            .OrderByDescending(s => s.InvoiceDate)
            .ThenByDescending(s => s.SOrderID)
            .Select(s => new SalesOrderListDto
            {
                SOrderID = s.SOrderID,
                InvoiceNo = s.InvoiceNo,
                InvoiceDate = s.InvoiceDate,
                CustomerID = s.CustomerID,
                CustomerCode = s.Customer != null ? s.Customer.Code : null,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                ContactNo = s.Customer != null ? s.Customer.ContactNo : null,
                GrandTotal = s.GrandTotal,
                TotalAmount = s.TotalAmount,
                RecAmount = s.RecAmount,
                PaymentDue = s.PaymentDue,
                TotalDue = s.TotalDue,
                NetDiscount = s.NetDiscount,
                AdjAmount = s.AdjAmount,
                Status = s.Status,
                MRNo = s.MRNo,
                LedgerNo = s.LedgerNo,
                REGMRNo = s.REGMRNo,
                Remarks = s.Remarks,
                ConcernID = s.ConcernID,
                TotalItemsCount = s.SOrderDetails.Count,
                TotalQuantity = s.SOrderDetails.Sum(d => d.Quantity)
            })
            .ToListAsync(cancellationToken);
    }
}
