using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Districts.Queries;

public record GetNextDistrictCodeQuery : IRequest<string>;

public class GetNextDistrictCodeQueryHandler : IRequestHandler<GetNextDistrictCodeQuery, string>
{
    private readonly IApplicationDbContext _context;

    public GetNextDistrictCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetNextDistrictCodeQuery request, CancellationToken cancellationToken)
    {
        var existingCodes = await _context.Districts
            .AsNoTracking()
            .Select(d => d.Code)
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
