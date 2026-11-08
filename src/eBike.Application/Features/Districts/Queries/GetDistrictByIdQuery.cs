using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Districts.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Districts.Queries;

public record GetDistrictByIdQuery(int Id) : IRequest<DistrictDto>;

public class GetDistrictByIdQueryHandler : IRequestHandler<GetDistrictByIdQuery, DistrictDto>
{
    private readonly IApplicationDbContext _context;

    public GetDistrictByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DistrictDto> Handle(GetDistrictByIdQuery request, CancellationToken cancellationToken)
    {
        var district = await _context.Districts
            .AsNoTracking()
            .Include(d => d.Thanas)
            .FirstOrDefaultAsync(d => d.DistrictID == request.Id, cancellationToken);

        if (district == null)
        {
            throw new NotFoundException(nameof(District), request.Id);
        }

        return new DistrictDto
        {
            DistrictID = district.DistrictID,
            Code = district.Code,
            Name = district.Name,
            ThanaCount = district.Thanas.Count
        };
    }
}
