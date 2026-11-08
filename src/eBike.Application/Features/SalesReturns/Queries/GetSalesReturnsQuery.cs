using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesReturns.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesReturns.Queries;

public record GetSalesReturnsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? CustomerId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<SalesReturnListDto>>;

public class GetSalesReturnsQueryHandler : IRequestHandler<GetSalesReturnsQuery, List<SalesReturnListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetSalesReturnsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<SalesReturnListDto>> Handle(GetSalesReturnsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SOrderDetails)
            .Where(s => s.Status == (int)EnumSalesType.ProductReturn || s.Status == (int)EnumSalesType.Return || s.InvoiceNo.StartsWith("SR-"));

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(s => s.ConcernID == targetConcernId.Value);
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            query = query.Where(s => s.CustomerID == request.CustomerId.Value);
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
                (s.Remarks != null && s.Remarks.ToLower().Contains(search)) ||
                (s.Customer != null && (
                    (s.Customer.Name != null && s.Customer.Name.ToLower().Contains(search)) ||
                    (s.Customer.ContactNo != null && s.Customer.ContactNo.ToLower().Contains(search))
                )));
        }

        var orders = await query
            .OrderByDescending(s => s.InvoiceDate)
            .ThenByDescending(s => s.SOrderID)
            .Select(s => new SalesReturnListDto
            {
                SOrderID = s.SOrderID,
                InvoiceNo = s.InvoiceNo,
                InvoiceDate = s.InvoiceDate,
                CustomerID = s.CustomerID,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                CustomerContact = s.Customer != null ? s.Customer.ContactNo : null,
                CustomerAddress = s.Customer != null ? s.Customer.Address : null,
                GrandTotal = s.GrandTotal,
                TotalAmount = s.TotalAmount,
                RecAmount = s.RecAmount,
                PaymentDue = s.PaymentDue,
                TotalDue = s.TotalDue,
                Status = s.Status,
                StatusName = "Sales Return",
                ItemCount = s.SOrderDetails.Count,
                Remarks = s.Remarks
            })
            .ToListAsync(cancellationToken);

        return orders;
    }
}
