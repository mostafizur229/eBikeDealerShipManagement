using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Departments.Queries;

public record GetNextDepartmentCodeQuery : IRequest<string>;

public class GetNextDepartmentCodeQueryHandler : IRequestHandler<GetNextDepartmentCodeQuery, string>
{
    private readonly IApplicationDbContext _context;

    public GetNextDepartmentCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetNextDepartmentCodeQuery request, CancellationToken cancellationToken)
    {
        var existingCodes = await _context.Departments
            .AsNoTracking()
            .Select(d => d.CODE)
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
