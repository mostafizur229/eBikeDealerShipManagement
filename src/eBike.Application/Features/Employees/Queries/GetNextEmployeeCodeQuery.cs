using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Employees.Queries;

public record GetNextEmployeeCodeQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextEmployeeCodeQueryHandler : IRequestHandler<GetNextEmployeeCodeQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextEmployeeCodeQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextEmployeeCodeQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Employees.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(e => e.ConcernID == targetConcernId.Value);
        }

        var existingCodes = await query
            .Select(e => e.Code)
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
