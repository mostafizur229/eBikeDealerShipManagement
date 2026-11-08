using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Designations.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Designations.Queries;

public record GetDesignationByIdQuery(int Id) : IRequest<DesignationDto>;

public class GetDesignationByIdQueryHandler : IRequestHandler<GetDesignationByIdQuery, DesignationDto>
{
    private readonly IApplicationDbContext _context;

    public GetDesignationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DesignationDto> Handle(GetDesignationByIdQuery request, CancellationToken cancellationToken)
    {
        var designation = await _context.Designations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DesignationID == request.Id, cancellationToken);

        if (designation == null)
        {
            throw new NotFoundException(nameof(Designation), request.Id);
        }

        return new DesignationDto
        {
            DesignationID = designation.DesignationID,
            Code = designation.Code,
            Description = designation.Description,
            CommissionPercent = designation.CommissionPercent
        };
    }
}
