using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Companies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Companies.Queries;

public record GetCompaniesQuery(
    string? SearchTerm = null,
    int? ConcernId = null
) : IRequest<List<CompanyDto>>;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCompaniesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Companies
            .Include(c => c.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        // Multi-tenancy filter: If specific concern requested, use it; otherwise fallback to current user's concern (if available)
        int? filterConcernId = request.ConcernId ?? _currentUser.ConcernId;
        if (filterConcernId.HasValue && filterConcernId.Value > 0)
        {
            query = query.Where(c => c.ConcernID == filterConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search));
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CompanyDto
            {
                Id = c.CompanyID,
                Code = c.Code,
                Name = c.Name,
                ConcernId = c.ConcernID,
                ConcernName = c.SisterConcern != null ? c.SisterConcern.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
