using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditInterestPercentages.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditInterestPercentages.Queries;

public record GetCreditInterestPercentageByIdQuery(int Id) : IRequest<CreditInterestPercentageDto?>;

public class GetCreditInterestPercentageByIdQueryHandler : IRequestHandler<GetCreditInterestPercentageByIdQuery, CreditInterestPercentageDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCreditInterestPercentageByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreditInterestPercentageDto?> Handle(GetCreditInterestPercentageByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.CreditInterestPercentages
            .Include(p => p.SisterConcern)
            .AsNoTracking()
            .Where(p => p.IntPercentageID == request.Id)
            .Select(p => new CreditInterestPercentageDto
            {
                IntPercentageId = p.IntPercentageID,
                Code = p.Code,
                Percentage = p.Percentage,
                EffectDate = p.EffectDate,
                ConcernId = p.ConcernID,
                ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
