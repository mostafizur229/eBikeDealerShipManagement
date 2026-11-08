using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseDOs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseDOs.Queries;

public record GetPurchaseDOsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? CustomerId = null,
    int? SupplierId = null,
    int? Status = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<PurchaseDOListDto>>;

public class GetPurchaseDOsQueryHandler : IRequestHandler<GetPurchaseDOsQuery, List<PurchaseDOListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseDOsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<PurchaseDOListDto>> Handle(GetPurchaseDOsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.DOes
            .Include(d => d.Customer)
            .Include(d => d.Supplier)
            .Include(d => d.DODetails)
            .AsNoTracking()
            .AsQueryable();

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(d => d.ConcernID == targetConcernId.Value);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(d => d.Date >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(d => d.Date <= to);
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            query = query.Where(d => d.CustomerID == request.CustomerId.Value);
        }

        if (request.SupplierId.HasValue && request.SupplierId.Value > 0)
        {
            query = query.Where(d => d.SupplierID == request.SupplierId.Value);
        }

        if (request.Status.HasValue && request.Status.Value > 0)
        {
            query = query.Where(d => d.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d =>
                d.DONo.ToLower().Contains(search) ||
                (d.Customer != null && d.Customer.Name.ToLower().Contains(search)) ||
                (d.Supplier != null && d.Supplier.Name.ToLower().Contains(search)) ||
                (d.Remarks != null && d.Remarks.ToLower().Contains(search)));
        }

        query = query.OrderByDescending(d => d.Date).ThenByDescending(d => d.DOID);

        return await query.Select(d => new PurchaseDOListDto
        {
            DOID = d.DOID,
            Date = d.Date,
            DONo = d.DONo,
            CustomerID = d.CustomerID,
            CustomerName = d.Customer != null ? d.Customer.Name : null,
            SupplierID = d.SupplierID,
            SupplierName = d.Supplier != null ? d.Supplier.Name : null,
            Status = d.Status,
            Remarks = d.Remarks,
            TotalAmt = d.TotalAmt,
            PaidAmt = d.PaidAmt,
            NetDiscount = d.NetDiscount,
            TotalTax = d.TotalTax,
            GrandTotal = d.GrandTotal,
            ConcernID = d.ConcernID,
            TotalItemsCount = d.DODetails != null ? d.DODetails.Count : 0
        }).ToListAsync(cancellationToken);
    }
}
