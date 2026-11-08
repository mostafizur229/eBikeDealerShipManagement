using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Queries;

public record GetNextThanaCodeQuery : IRequest<string>;

public class GetNextThanaCodeQueryHandler : IRequestHandler<GetNextThanaCodeQuery, string>
{
    private readonly IApplicationDbContext _context;

    public GetNextThanaCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetNextThanaCodeQuery request, CancellationToken cancellationToken)
    {
        var existingCodes = await _context.Thanas
            .AsNoTracking()
            .Select(t => t.Code)
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
