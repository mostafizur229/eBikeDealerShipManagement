using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Expenditures.Queries;

public record GetNextVoucherNoQuery(int? ConcernId = null) : IRequest<string>;

public class GetNextVoucherNoQueryHandler : IRequestHandler<GetNextVoucherNoQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetNextVoucherNoQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(GetNextVoucherNoQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Expenditures.AsNoTracking();
        if (targetConcernId.HasValue)
        {
            query = query.Where(e => e.ConcernID == targetConcernId.Value);
        }

        var existingCodes = await query
            .Select(e => e.VoucherNo)
            .ToListAsync(cancellationToken);

        int maxNo = 0;
        foreach (var no in existingCodes)
        {
            if (int.TryParse(no, out int parsed))
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
