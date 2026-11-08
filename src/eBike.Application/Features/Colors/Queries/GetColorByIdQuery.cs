using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Colors.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Colors.Queries;

public record GetColorByIdQuery(int Id) : IRequest<ColorDto>;

public class GetColorByIdQueryHandler : IRequestHandler<GetColorByIdQuery, ColorDto>
{
    private readonly IApplicationDbContext _context;

    public GetColorByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ColorDto> Handle(GetColorByIdQuery request, CancellationToken cancellationToken)
    {
        var color = await _context.Colors
            .Include(c => c.SisterConcern)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ColorID == request.Id, cancellationToken);

        if (color == null)
        {
            throw new NotFoundException(nameof(Color), request.Id);
        }

        return new ColorDto
        {
            Id = color.ColorID,
            Code = color.Code,
            Name = color.Name,
            ConcernId = color.ConcernID,
            ConcernName = color.SisterConcern != null ? color.SisterConcern.Name : null
        };
    }
}
