using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseDOs.Queries;

public record GetNextDONoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextDONoQueryHandler : IRequestHandler<GetNextDONoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextDONoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextDONoQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.DOes.AsNoTracking();
        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(d => d.ConcernID == targetConcernId.Value);
        }

        var maxId = await query.Select(d => d.DOID).DefaultIfEmpty(0).MaxAsync(cancellationToken);
        return $"DO-{(maxId + 1):D6}";
    }
}
