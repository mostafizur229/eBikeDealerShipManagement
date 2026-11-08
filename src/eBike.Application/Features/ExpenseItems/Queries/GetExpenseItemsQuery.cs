using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ExpenseItems.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ExpenseItems.Queries;

public record GetExpenseItemsQuery(
    string? Search = null,
    int? Status = null,
    int? ConcernId = null
) : IRequest<List<ExpenseItemListDto>>;

public class GetExpenseItemsQueryHandler : IRequestHandler<GetExpenseItemsQuery, List<ExpenseItemListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetExpenseItemsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ExpenseItemListDto>> Handle(GetExpenseItemsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.ExpenseItems
            .AsNoTracking()
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(e => e.ConcernID == targetConcernId.Value);
        }

        if (request.Status.HasValue && request.Status.Value > 0)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e =>
                (e.Description != null && e.Description.ToLower().Contains(search)) ||
                (e.Code != null && e.Code.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(e => e.Code)
            .Select(e => new ExpenseItemListDto
            {
                ExpenseItemID = e.ExpenseItemID,
                Code = e.Code,
                Description = e.Description,
                Status = e.Status,
                StatusName = ((EnumCompanyTransaction)e.Status).ToString(),
                ConcernID = e.ConcernID
            })
            .ToListAsync(cancellationToken);
    }
}
