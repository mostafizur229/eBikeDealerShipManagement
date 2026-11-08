using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Banks.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Banks.Queries;

public record GetBankByIdQuery(int Id) : IRequest<BankDto>;

public class GetBankByIdQueryHandler : IRequestHandler<GetBankByIdQuery, BankDto>
{
    private readonly IApplicationDbContext _context;

    public GetBankByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankDto> Handle(GetBankByIdQuery request, CancellationToken cancellationToken)
    {
        var bank = await _context.Banks
            .AsNoTracking()
            .Include(b => b.SisterConcern)
            .FirstOrDefaultAsync(b => b.BankID == request.Id, cancellationToken);

        if (bank == null)
        {
            throw new NotFoundException(nameof(Bank), request.Id);
        }

        return new BankDto
        {
            BankID = bank.BankID,
            Code = bank.Code,
            BankName = bank.BankName,
            AccountNo = bank.AccountNo,
            BranchName = bank.BranchName,
            AccountName = bank.AccountName,
            OpeningBalance = bank.OpeningBalance,
            TotalAmount = bank.TotalAmount,
            IsAdvancedDueLimitApplicable = bank.IsAdvancedDueLimitApplicable == 1,
            AdvancedAmountLimit = bank.AdvancedAmountLimit ?? 0m,
            ConcernID = bank.ConcernID,
            ConcernName = bank.SisterConcern?.Name
        };
    }
}
