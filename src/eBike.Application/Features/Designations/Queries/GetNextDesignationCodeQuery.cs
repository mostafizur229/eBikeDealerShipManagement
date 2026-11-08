using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Designations.Queries;

public record GetNextDesignationCodeQuery : IRequest<string>;

public class GetNextDesignationCodeQueryHandler : IRequestHandler<GetNextDesignationCodeQuery, string>
{
    private readonly IApplicationDbContext _context;

    public GetNextDesignationCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetNextDesignationCodeQuery request, CancellationToken cancellationToken)
    {
        var existingCodes = await _context.Designations
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
