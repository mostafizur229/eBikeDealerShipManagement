using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesReturns.Queries;

public record GetNextSalesReturnInvoiceNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextSalesReturnInvoiceNoQueryHandler : IRequestHandler<GetNextSalesReturnInvoiceNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextSalesReturnInvoiceNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextSalesReturnInvoiceNoQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var lastInvoice = await _context.SOrders
            .AsNoTracking()
            .Where(s => s.ConcernID == targetConcernId && s.InvoiceNo.StartsWith("SR-"))
            .OrderByDescending(s => s.SOrderID)
            .Select(s => s.InvoiceNo)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNum = 1;
        if (!string.IsNullOrEmpty(lastInvoice))
        {
            var clean = lastInvoice.Replace("SR-", "").Trim();
            if (int.TryParse(clean, out int parsed))
                nextNum = parsed + 1;
        }

        return $"SR-{nextNum:D4}";
    }
}
