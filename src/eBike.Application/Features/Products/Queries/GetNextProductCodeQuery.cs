using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Products.Queries;

public record GetNextProductCodeQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextProductCodeQueryHandler : IRequestHandler<GetNextProductCodeQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextProductCodeQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextProductCodeQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Products.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(p => p.ConcernID == targetConcernId.Value);
        }

        var existingCodes = await query
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        int maxCode = 0;
        foreach (var code in existingCodes)
        {
            if (int.TryParse(code, out int parsed))
            {
                if (parsed > maxCode)
                {
                    maxCode = parsed;
                }
            }
        }

        return (maxCode + 1).ToString("D5");
    }
}
