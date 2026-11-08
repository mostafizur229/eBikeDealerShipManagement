using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashDeliveries.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashDeliveries.Queries;

public record GetCashDeliveryByIdQuery(int Id) : IRequest<CashDeliveryDto?>;

public class GetCashDeliveryByIdQueryHandler : IRequestHandler<GetCashDeliveryByIdQuery, CashDeliveryDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCashDeliveryByIdQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CashDeliveryDto?> Handle(GetCashDeliveryByIdQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = _currentUser.ConcernId;

        var query = _context.CashCollections
            .AsNoTracking()
            .Include(c => c.Supplier)
            .Where(c => c.CashCollectionID == request.Id &&
                       (c.TransactionType == (int)EnumTranType.ToCompany || c.TransactionType == (int)EnumTranType.DeliveryPending));

        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        var c = await query.FirstOrDefaultAsync(cancellationToken);
        if (c == null)
            return null;

        return new CashDeliveryDto
        {
            CashCollectionID = c.CashCollectionID,
            ReceiptNo = c.ReceiptNo ?? string.Empty,
            EntryDate = c.EntryDate,
            SupplierID = c.SupplierID,
            SupplierName = c.Supplier?.Name,
            SupplierCode = c.Supplier?.Code,
            Amount = c.Amount ?? 0,
            AdjustAmt = c.AdjustAmt,
            BalanceDue = c.BalanceDue,
            PaymentType = c.PaymentType ?? 1,
            AccountNo = c.AccountNo,
            BankName = c.BankName,
            BranchName = c.BranchName,
            MBAccountNo = c.MBAccountNo,
            BKashNo = c.BKashNo,
            Remarks = c.Remarks,
            TransactionType = c.TransactionType ?? (int)EnumTranType.ToCompany,
            ConcernID = c.ConcernID,
            CreatedBy = c.CreatedBy,
            CreateDate = c.CreateDate
        };
    }
}
