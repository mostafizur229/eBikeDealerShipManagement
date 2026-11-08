using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Religions.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Religions.Queries;

public record GetReligionByIdQuery(int Id) : IRequest<ReligionDto>;

public class GetReligionByIdQueryHandler : IRequestHandler<GetReligionByIdQuery, ReligionDto>
{
    private readonly IApplicationDbContext _context;

    public GetReligionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReligionDto> Handle(GetReligionByIdQuery request, CancellationToken cancellationToken)
    {
        var religion = await _context.Religions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReligionID == request.Id, cancellationToken);

        if (religion == null)
        {
            throw new NotFoundException(nameof(Religion), request.Id);
        }

        return new ReligionDto
        {
            ReligionID = religion.ReligionID,
            Code = religion.Code,
            Name = religion.Name,
            SequenceNO = religion.SequenceNO,
            Status = religion.Status
        };
    }
}
