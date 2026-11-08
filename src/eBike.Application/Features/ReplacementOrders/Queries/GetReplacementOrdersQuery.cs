using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ReplacementOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Queries;

public record GetReplacementOrdersQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? CustomerId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<ReplacementOrderListDto>>;

public class GetReplacementOrdersQueryHandler : IRequestHandler<GetReplacementOrdersQuery, List<ReplacementOrderListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetReplacementOrdersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ReplacementOrderListDto>> Handle(GetReplacementOrdersQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SOrderDetails)
            .Where(s => s.IsReplacement == 1);

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
                (s.Customer != null && (s.Customer.Name.ToLower().Contains(search) || (s.Customer.ContactNo != null && s.Customer.ContactNo.Contains(search))))
            );
        }

        var orders = await query
            .OrderByDescending(s => s.SOrderID)
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.SOrderID).ToList();

        // Query original damage details linked via RepOrderID
        var damageDetails = await _context.SOrderDetails
            .AsNoTracking()
            .Where(d => d.RepOrderID.HasValue && orderIds.Contains(d.RepOrderID.Value))
            .GroupBy(d => d.RepOrderID!.Value)
            .Select(g => new
            {
                RepOrderId = g.Key,
                TotalDamage = g.Sum(x => x.UnitPrice)
            })
            .ToDictionaryAsync(x => x.RepOrderId, x => x.TotalDamage, cancellationToken);

        return orders.Select(o =>
        {
            decimal damageTotal = damageDetails.TryGetValue(o.SOrderID, out var dTotal) ? dTotal : 0;
            decimal replaceTotal = o.SOrderDetails.Sum(d => d.UTAmount > 0 ? d.UTAmount : (d.UnitPrice * d.Quantity));

            return new ReplacementOrderListDto
            {
                SOrderId = o.SOrderID,
                InvoiceNo = o.InvoiceNo,
                InvoiceDate = o.InvoiceDate,
                CustomerId = o.CustomerID,
                CustomerName = o.Customer != null ? o.Customer.Name : "N/A",
                ContactNo = o.Customer?.ContactNo,
                DamageTotalAmount = damageTotal,
                ReplaceTotalAmount = replaceTotal,
                GrandTotal = o.GrandTotal != 0 ? o.GrandTotal : (replaceTotal - damageTotal),
                AdjAmount = o.AdjAmount,
                TotalAmount = o.TotalAmount,
                RecAmount = o.RecAmount ?? 0,
                PaymentDue = o.PaymentDue,
                TotalDue = o.Customer != null ? o.Customer.TotalDue : 0,
                ItemCount = o.SOrderDetails.Count,
                Status = "Replacement"
            };
        }).ToList();
    }
}
