using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PLocations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PLocations.Queries;

public record GetPLocationByIdQuery(int Id) : IRequest<PLocationDto?>;

public class GetPLocationByIdQueryHandler : IRequestHandler<GetPLocationByIdQuery, PLocationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPLocationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PLocationDto?> Handle(GetPLocationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.PLocations
            .Include(p => p.SisterConcern)
            .AsNoTracking()
            .Where(p => p.PLocationID == request.Id)
            .Select(p => new PLocationDto
            {
                Id = p.PLocationID,
                Code = p.Code,
                Description = p.Description,
                ConcernId = p.ConcernID,
                ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
