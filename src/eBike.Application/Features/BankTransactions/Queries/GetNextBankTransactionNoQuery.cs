using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.BankTransactions.Queries;

public record GetNextBankTransactionNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextBankTransactionNoQueryHandler : IRequestHandler<GetNextBankTransactionNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextBankTransactionNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextBankTransactionNoQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.BankTransactions.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(bt => bt.ConcernID == targetConcernId.Value);
        }

        var existingCodes = await query
            .Select(bt => bt.TransactionNo)
            .ToListAsync(cancellationToken);

        int maxNo = 0;
        foreach (var no in existingCodes)
        {
            if (int.TryParse(no, out int parsed))
            {
                if (parsed > maxNo)
                {
                    maxNo = parsed;
                }
            }
        }

        return (maxNo + 1).ToString("D5");
    }
}
