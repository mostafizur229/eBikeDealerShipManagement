using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashCollections.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Queries;

public record GetCashCollectionsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? CustomerId = null,
    int? TransactionType = null,
    string? Search = null,
    int? ConcernId = null,
    int? ProjectType = null
) : IRequest<List<CashCollectionListDto>>;

public class GetCashCollectionsQueryHandler : IRequestHandler<GetCashCollectionsQuery, List<CashCollectionListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCashCollectionsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CashCollectionListDto>> Handle(GetCashCollectionsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.CashCollections
            .AsNoTracking()
            .Include(c => c.Customer)
            .Where(c => c.CustomerID != null && c.CustomerID > 0 &&
                       c.TransactionType != (int)EnumTranType.ToCompany &&
                       c.TransactionType != (int)EnumTranType.DeliveryPending)
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        if (request.ProjectType.HasValue && request.ProjectType.Value > 0)
        {
            query = query.Where(c => c.ProjectType == request.ProjectType.Value);
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            query = query.Where(c => c.CustomerID == request.CustomerId.Value);
        }

        if (request.TransactionType.HasValue && request.TransactionType.Value > 0)
        {
            query = query.Where(c => c.TransactionType == request.TransactionType.Value);
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
                (c.MRNo != null && c.MRNo.ToLower().Contains(search)) ||
                (c.Remarks != null && c.Remarks.ToLower().Contains(search)) ||
                (c.Customer != null && (
                    (c.Customer.Name != null && c.Customer.Name.ToLower().Contains(search)) ||
                    (c.Customer.Code != null && c.Customer.Code.ToLower().Contains(search)) ||
                    (c.Customer.ContactNo != null && c.Customer.ContactNo.ToLower().Contains(search))
                )));
        }

        return await query
            .OrderByDescending(c => c.EntryDate)
            .ThenByDescending(c => c.CashCollectionID)
            .Select(c => new CashCollectionListDto
            {
                CashCollectionID = c.CashCollectionID,
                ReceiptNo = c.ReceiptNo ?? string.Empty,
                EntryDate = c.EntryDate,
                CustomerID = c.CustomerID,
                CustomerCode = c.Customer != null ? c.Customer.Code : null,
                CustomerName = c.Customer != null ? c.Customer.Name : null,
                AccountNo = c.AccountNo,
                Amount = c.Amount ?? 0,
                AdjustAmt = c.AdjustAmt,
                InterestAmt = c.InterestAmt,
                BalanceDue = c.BalanceDue,
                PaymentType = c.PaymentType ?? 1,
                TransactionType = c.TransactionType ?? (int)EnumTranType.FromCustomer,
                ProjectType = c.ProjectType,
                MRNo = c.MRNo,
                Remarks = c.Remarks
            })
            .ToListAsync(cancellationToken);
    }
}
