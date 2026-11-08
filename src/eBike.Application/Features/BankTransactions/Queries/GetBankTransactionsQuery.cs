using eBike.Application.Common.Interfaces;
using eBike.Application.Features.BankTransactions.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.BankTransactions.Queries;

public record GetBankTransactionsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? TransactionType = null,
    int? BankId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<BankTransactionListDto>>;

public class GetBankTransactionsQueryHandler : IRequestHandler<GetBankTransactionsQuery, List<BankTransactionListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetBankTransactionsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BankTransactionListDto>> Handle(GetBankTransactionsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.BankTransactions
            .AsNoTracking()
            .Include(bt => bt.Bank)
            .Include(bt => bt.AnotherBank)
            .Include(bt => bt.Customer)
            .Include(bt => bt.Supplier)
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(bt => bt.ConcernID == targetConcernId.Value);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(bt => bt.TranDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date;
            query = query.Where(bt => bt.TranDate <= to);
        }

        if (request.TransactionType.HasValue && request.TransactionType.Value > 0)
        {
            query = query.Where(bt => bt.TransactionType == request.TransactionType.Value);
        }

        if (request.BankId.HasValue && request.BankId.Value > 0)
        {
            query = query.Where(bt => bt.BankID == request.BankId.Value || bt.AnotherBankID == request.BankId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(bt =>
                (bt.TransactionNo != null && bt.TransactionNo.ToLower().Contains(search)) ||
                (bt.ChecqueNo != null && bt.ChecqueNo.ToLower().Contains(search)) ||
                (bt.Remarks != null && bt.Remarks.ToLower().Contains(search)) ||
                (bt.Bank != null && bt.Bank.BankName != null && bt.Bank.BankName.ToLower().Contains(search)) ||
                (bt.Customer != null && bt.Customer.Name != null && bt.Customer.Name.ToLower().Contains(search)) ||
                (bt.Supplier != null && bt.Supplier.Name != null && bt.Supplier.Name.ToLower().Contains(search)));
        }

        return await query
            .OrderByDescending(bt => bt.TranDate)
            .ThenByDescending(bt => bt.BankTranID)
            .Select(bt => new BankTransactionListDto
            {
                BankTranID = bt.BankTranID,
                TranDate = bt.TranDate,
                TransactionNo = bt.TransactionNo,
                TransactionType = bt.TransactionType,
                TransactionTypeName = ((EnumTransactionType)bt.TransactionType).ToString(),
                Amount = bt.Amount,
                BankID = bt.BankID,
                BankName = bt.Bank != null ? bt.Bank.BankName : null,
                AccountNo = bt.Bank != null ? bt.Bank.AccountNo : null,
                AnotherBankID = bt.AnotherBankID,
                AnotherBankName = bt.AnotherBank != null ? bt.AnotherBank.BankName : null,
                CustomerID = bt.CustomerID,
                CustomerName = bt.Customer != null ? bt.Customer.Name : null,
                SupplierID = bt.SupplierID,
                SupplierName = bt.Supplier != null ? bt.Supplier.Name : null,
                ChecqueNo = bt.ChecqueNo,
                Remarks = bt.Remarks,
                WFStatus = bt.WFStatus,
                WFStatusName = ((EnumWFStatus)bt.WFStatus).ToString(),
                ConcernID = bt.ConcernID
            })
            .ToListAsync(cancellationToken);
    }
}
