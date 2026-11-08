using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Companies.Queries;

public record GetNextCompanyCodeQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextCompanyCodeQueryHandler : IRequestHandler<GetNextCompanyCodeQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextCompanyCodeQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextCompanyCodeQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Companies.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        var existingCodes = await query
            .Select(c => c.Code)
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
