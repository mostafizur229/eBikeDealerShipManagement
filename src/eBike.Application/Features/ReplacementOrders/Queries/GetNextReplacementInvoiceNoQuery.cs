using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Queries;

public record GetNextReplacementInvoiceNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextReplacementInvoiceNoQueryHandler : IRequestHandler<GetNextReplacementInvoiceNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextReplacementInvoiceNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextReplacementInvoiceNoQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var existingInvoices = await _context.SOrders
            .AsNoTracking()
            .Where(s => s.ConcernID == targetConcernId && s.IsReplacement == 1 && s.InvoiceNo.StartsWith("RO-"))
            .Select(s => s.InvoiceNo)
            .ToListAsync(cancellationToken);

        int maxSeq = 0;
        foreach (var inv in existingInvoices)
        {
            var part = inv.Replace("RO-", "").Trim();
            if (int.TryParse(part, out int seq) && seq > maxSeq)
            {
                maxSeq = seq;
            }
        }

        return $"RO-{(maxSeq + 1):D4}";
    }
}
