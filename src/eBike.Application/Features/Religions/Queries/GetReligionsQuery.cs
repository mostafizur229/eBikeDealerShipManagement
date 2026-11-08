using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Religions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Religions.Queries;

public record GetReligionsQuery : IRequest<List<ReligionListDto>>;

public class GetReligionsQueryHandler : IRequestHandler<GetReligionsQuery, List<ReligionListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReligionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReligionListDto>> Handle(GetReligionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Religions
            .AsNoTracking()
            .OrderBy(r => r.SequenceNO)
            .ThenBy(r => r.ReligionID)
            .Select(r => new ReligionListDto
            {
                ReligionID = r.ReligionID,
                Code = r.Code,
                Name = r.Name,
                SequenceNO = r.SequenceNO,
                Status = r.Status
            })
            .ToListAsync(cancellationToken);
    }
}
