using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashDeliveries.Queries;

public record GetNextCashDeliveryReceiptNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextCashDeliveryReceiptNoQueryHandler : IRequestHandler<GetNextCashDeliveryReceiptNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextCashDeliveryReceiptNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextCashDeliveryReceiptNoQuery request, CancellationToken cancellationToken)
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
