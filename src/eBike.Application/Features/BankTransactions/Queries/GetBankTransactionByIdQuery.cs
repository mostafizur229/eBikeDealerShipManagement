using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.BankTransactions.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.BankTransactions.Queries;

public record GetBankTransactionByIdQuery(int Id) : IRequest<BankTransactionDto>;

public class GetBankTransactionByIdQueryHandler : IRequestHandler<GetBankTransactionByIdQuery, BankTransactionDto>
{
    private readonly IApplicationDbContext _context;

    public GetBankTransactionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankTransactionDto> Handle(GetBankTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var bt = await _context.BankTransactions
            .AsNoTracking()
            .Include(t => t.Bank)
            .Include(t => t.AnotherBank)
            .Include(t => t.Customer)
            .Include(t => t.Supplier)
            .Include(t => t.SisterConcern)
            .FirstOrDefaultAsync(t => t.BankTranID == request.Id, cancellationToken);

        if (bt == null)
        {
            throw new NotFoundException(nameof(BankTransaction), request.Id);
        }

        return new BankTransactionDto
        {
            BankTranID = bt.BankTranID,
            TranDate = bt.TranDate,
            TransactionNo = bt.TransactionNo,
            TransactionType = bt.TransactionType,
            TransactionTypeName = ((EnumTransactionType)bt.TransactionType).ToString(),
            Amount = bt.Amount,
            BankID = bt.BankID,
            BankName = bt.Bank?.BankName,
            AccountNo = bt.Bank?.AccountNo,
            AccountName = bt.Bank?.AccountName,
            AnotherBankID = bt.AnotherBankID,
            AnotherBankName = bt.AnotherBank?.BankName,
            AnotherAccountNo = bt.AnotherBank?.AccountNo,
            CustomerID = bt.CustomerID,
            CustomerName = bt.Customer?.Name,
            CustomerCode = bt.Customer?.Code,
            SupplierID = bt.SupplierID,
            SupplierName = bt.Supplier?.Name,
            ChecqueNo = bt.ChecqueNo,
            Remarks = bt.Remarks,
            ExpenseItemID = bt.ExpenseItemID,
            SIHID = bt.SIHID,
            WFStatus = bt.WFStatus,
            WFStatusName = ((EnumWFStatus)bt.WFStatus).ToString(),
            ConcernID = bt.ConcernID,
            ConcernName = bt.SisterConcern?.Name
        };
    }
}
