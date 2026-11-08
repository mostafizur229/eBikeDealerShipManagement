using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Queries;

public record GetNextCashCollectionReceiptNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextCashCollectionReceiptNoQueryHandler : IRequestHandler<GetNextCashCollectionReceiptNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextCashCollectionReceiptNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextCashCollectionReceiptNoQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.CashCollections.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        var existingReceipts = await query
            .Where(c => c.ReceiptNo != null)
            .Select(c => c.ReceiptNo!)
            .ToListAsync(cancellationToken);

        int maxNo = 0;
        foreach (var receipt in existingReceipts)
        {
            if (int.TryParse(receipt, out int parsed))
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
