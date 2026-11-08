using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashDeliveries.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashDeliveries.Queries;

public record GetCashDeliveriesQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? SupplierId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<CashDeliveryListDto>>;

public class GetCashDeliveriesQueryHandler : IRequestHandler<GetCashDeliveriesQuery, List<CashDeliveryListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCashDeliveriesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CashDeliveryListDto>> Handle(GetCashDeliveriesQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.CashCollections
            .AsNoTracking()
            .Include(c => c.Supplier)
            .Where(c => c.SupplierID != null && c.SupplierID > 0 &&
                       (c.TransactionType == (int)EnumTranType.ToCompany || c.TransactionType == (int)EnumTranType.DeliveryPending))
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        if (request.SupplierId.HasValue && request.SupplierId.Value > 0)
        {
            query = query.Where(c => c.SupplierID == request.SupplierId.Value);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(c => c.EntryDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date;
            query = query.Where(c => c.EntryDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(c =>
                (c.ReceiptNo != null && c.ReceiptNo.ToLower().Contains(search)) ||
                (c.AccountNo != null && c.AccountNo.ToLower().Contains(search)) ||
                (c.Remarks != null && c.Remarks.ToLower().Contains(search)) ||
                (c.Supplier != null && c.Supplier.Name != null && c.Supplier.Name.ToLower().Contains(search)));
        }

        return await query
            .OrderByDescending(c => c.EntryDate)
            .ThenByDescending(c => c.CashCollectionID)
            .Select(c => new CashDeliveryListDto
            {
                CashCollectionID = c.CashCollectionID,
                ReceiptNo = c.ReceiptNo ?? string.Empty,
                EntryDate = c.EntryDate,
                SupplierID = c.SupplierID,
                SupplierName = c.Supplier != null ? c.Supplier.Name : null,
                SupplierCode = c.Supplier != null ? c.Supplier.Code : null,
                AccountNo = c.AccountNo,
                Amount = c.Amount ?? 0,
                AdjustAmt = c.AdjustAmt,
                BalanceDue = c.BalanceDue,
                PaymentType = c.PaymentType ?? 1,
                TransactionType = c.TransactionType ?? (int)EnumTranType.ToCompany,
                Remarks = c.Remarks
            })
            .ToListAsync(cancellationToken);
    }
}
