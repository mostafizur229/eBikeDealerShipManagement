using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Categories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Categories.Queries;

public record GetCategoriesQuery(
    string? SearchTerm = null,
    int? ConcernId = null
) : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCategoriesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories
            .Include(c => c.SisterConcern)
            .AsNoTracking()
            .AsQueryable();

        int? filterConcernId = request.ConcernId ?? _currentUser.ConcernId;
        if (filterConcernId.HasValue && filterConcernId.Value > 0)
        {
            query = query.Where(c => c.ConcernID == filterConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Description.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search));
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CategoryDto
            {
                Id = c.CategoryID,
                Code = c.Code,
                Name = c.Description,
                ConcernId = c.ConcernID,
                ConcernName = c.SisterConcern != null ? c.SisterConcern.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
