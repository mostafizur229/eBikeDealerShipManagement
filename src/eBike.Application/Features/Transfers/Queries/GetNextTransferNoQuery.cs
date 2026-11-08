using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Queries;

public record GetNextTransferNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextTransferNoQueryHandler : IRequestHandler<GetNextTransferNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextTransferNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextTransferNoQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var existingInvoices = await _context.Transfers
            .AsNoTracking()
            .Where(t => (t.FromConcernID == targetConcernId || t.ConcernID == targetConcernId) && t.TransferNo.StartsWith("TR-"))
            .Select(t => t.TransferNo)
            .ToListAsync(cancellationToken);

        int maxSeq = 0;
        foreach (var inv in existingInvoices)
        {
            var part = inv.Replace("TR-", "").Trim();
            if (int.TryParse(part, out int seq) && seq > maxSeq)
            {
                maxSeq = seq;
            }
        }

        return $"TR-{(maxSeq + 1):D4}";
    }
}
