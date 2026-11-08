using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Queries;

public record GetTransferConcernsQuery : IRequest<List<BranchConcernDto>>;

public class GetTransferConcernsQueryHandler : IRequestHandler<GetTransferConcernsQuery, List<BranchConcernDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransferConcernsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BranchConcernDto>> Handle(GetTransferConcernsQuery request, CancellationToken cancellationToken)
    {
        return await _context.SisterConcerns
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new BranchConcernDto
            {
                ConcernId = c.ConcernID,
                Name = c.Name,
                Address = c.Address,
                ContactNo = c.ContactNo
            })
            .ToListAsync(cancellationToken);
    }
}
