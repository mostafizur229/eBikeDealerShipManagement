using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditInterestPercentages.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditInterestPercentages.Queries;

public record GetCreditInterestPercentagesQuery(
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<CreditInterestPercentageDto>>;

public class GetCreditInterestPercentagesQueryHandler : IRequestHandler<GetCreditInterestPercentagesQuery, List<CreditInterestPercentageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCreditInterestPercentagesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CreditInterestPercentageDto>> Handle(GetCreditInterestPercentagesQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.CreditInterestPercentages
            .Include(p => p.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            query = query.Where(p => p.ConcernID == targetConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Code.ToLower().Contains(search) ||
                p.Percentage.ToString().Contains(search));
        }

        query = query.OrderByDescending(p => p.EffectDate).ThenBy(p => p.Code);

        return await query.Select(p => new CreditInterestPercentageDto
        {
            IntPercentageId = p.IntPercentageID,
            Code = p.Code,
            Percentage = p.Percentage,
            EffectDate = p.EffectDate,
            ConcernId = p.ConcernID,
            ConcernName = p.SisterConcern != null ? p.SisterConcern.Name : null
        }).ToListAsync(cancellationToken);
    }
}
