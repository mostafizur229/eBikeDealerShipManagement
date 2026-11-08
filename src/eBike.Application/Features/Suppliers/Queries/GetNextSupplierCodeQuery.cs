using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Suppliers.Queries;

public record GetNextSupplierCodeQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextSupplierCodeQueryHandler : IRequestHandler<GetNextSupplierCodeQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextSupplierCodeQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextSupplierCodeQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Suppliers.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(s => s.ConcernID == targetConcernId.Value);
        }

        var existingCodes = await query
            .Select(s => s.Code)
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
