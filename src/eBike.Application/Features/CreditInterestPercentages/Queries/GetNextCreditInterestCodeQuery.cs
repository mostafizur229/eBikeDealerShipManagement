using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditInterestPercentages.Queries;

public record GetNextCreditInterestCodeQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextCreditInterestCodeQueryHandler : IRequestHandler<GetNextCreditInterestCodeQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextCreditInterestCodeQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextCreditInterestCodeQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.CreditInterestPercentages.AsNoTracking();
        if (targetConcernId.HasValue && targetConcernId.Value > 0)
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

        return (maxCode + 1).ToString("D4");
    }
}
