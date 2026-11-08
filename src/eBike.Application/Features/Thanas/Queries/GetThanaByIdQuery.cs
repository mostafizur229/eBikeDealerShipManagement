using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Thanas.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Queries;

public record GetThanaByIdQuery(int Id) : IRequest<ThanaDto>;

public class GetThanaByIdQueryHandler : IRequestHandler<GetThanaByIdQuery, ThanaDto>
{
    private readonly IApplicationDbContext _context;

    public GetThanaByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ThanaDto> Handle(GetThanaByIdQuery request, CancellationToken cancellationToken)
    {
        var thana = await _context.Thanas
            .AsNoTracking()
            .Include(t => t.District)
            .FirstOrDefaultAsync(t => t.ThanaID == request.Id, cancellationToken);

        if (thana == null)
        {
            throw new NotFoundException(nameof(Thana), request.Id);
        }

        return new ThanaDto
        {
            ThanaID = thana.ThanaID,
            Code = thana.Code,
            Name = thana.Name,
            DistrictID = thana.DistrictID,
            DistrictName = thana.District?.Name
        };
    }
}
