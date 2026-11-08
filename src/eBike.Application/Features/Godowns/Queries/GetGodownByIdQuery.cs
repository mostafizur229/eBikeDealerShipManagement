using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Godowns.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Godowns.Queries;

public record GetGodownByIdQuery(int Id) : IRequest<GodownDto>;

public class GetGodownByIdQueryHandler : IRequestHandler<GetGodownByIdQuery, GodownDto>
{
    private readonly IApplicationDbContext _context;

    public GetGodownByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GodownDto> Handle(GetGodownByIdQuery request, CancellationToken cancellationToken)
    {
        var godown = await _context.Godowns
            .Include(g => g.SisterConcern)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GodownID == request.Id, cancellationToken);

        if (godown == null)
        {
            throw new NotFoundException(nameof(Godown), request.Id);
        }

        return new GodownDto
        {
            Id = godown.GodownID,
            Code = godown.Code,
            Name = godown.Name,
            Address = godown.Address,
            ConcernId = godown.ConcernID,
            ConcernName = godown.SisterConcern != null ? godown.SisterConcern.Name : null,
            IsCommon = godown.ISCommon == 1
        };
    }
}
