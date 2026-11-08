using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashCollections.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Queries;

public record GetCashCollectionByIdQuery(int Id) : IRequest<CashCollectionDto?>;

public class GetCashCollectionByIdQueryHandler : IRequestHandler<GetCashCollectionByIdQuery, CashCollectionDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCashCollectionByIdQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CashCollectionDto?> Handle(GetCashCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = _currentUser.ConcernId;

        var query = _context.CashCollections
            .AsNoTracking()
            .Include(c => c.Customer)
            .Where(c => c.CashCollectionID == request.Id &&
                       c.CustomerID != null && c.CustomerID > 0 &&
                       c.TransactionType != (int)EnumTranType.ToCompany &&
                       c.TransactionType != (int)EnumTranType.DeliveryPending);

        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        var c = await query.FirstOrDefaultAsync(cancellationToken);
        if (c == null)
            return null;

        return new CashCollectionDto
        {
            CashCollectionID = c.CashCollectionID,
            ReceiptNo = c.ReceiptNo ?? string.Empty,
            EntryDate = c.EntryDate,
            CustomerID = c.CustomerID,
            CustomerCode = c.Customer?.Code,
            CustomerName = c.Customer?.Name,
            CustomerContactNo = c.Customer?.ContactNo,
            Amount = c.Amount ?? 0m,
            AdjustAmt = c.AdjustAmt,
            InterestAmt = c.InterestAmt,
            BalanceDue = c.BalanceDue,
            PaymentType = c.PaymentType ?? 1,
            AccountNo = c.AccountNo,
            BankName = c.BankName,
            BranchName = c.BranchName,
            MBAccountNo = c.MBAccountNo,
            BKashNo = c.BKashNo,
            MRNo = c.MRNo,
            Remarks = c.Remarks,
            RemindDate = c.Customer?.RemindDate,
            TransactionType = c.TransactionType ?? (int)EnumTranType.FromCustomer,
            ProjectType = c.ProjectType,
            ConcernID = c.ConcernID,
            CreatedBy = c.CreatedBy,
            CreateDate = c.CreateDate
        };
    }
}
