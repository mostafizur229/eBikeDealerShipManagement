using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Parts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Parts.Queries;

public record GetPartByIdQuery(int Id) : IRequest<PartDto?>;

public class GetPartByIdQueryHandler : IRequestHandler<GetPartByIdQuery, PartDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPartByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PartDto?> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Parts
            .Include(p => p.SisterConcern)
            .AsNoTracking()
            .Where(p => p.PartsNoID == request.Id)
            .Select(p => new PartDto
            {
                Id = p.PartsNoID,
                Code = p.Code,
                Description = p.Description,
                ConcernId = p.ConcernID,
                ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
