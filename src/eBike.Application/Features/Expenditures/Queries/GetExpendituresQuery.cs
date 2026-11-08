using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Expenditures.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Expenditures.Queries;

public record GetExpendituresQuery(
    int? Type = null, // 1: Expense, 2: Income, null: all
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? ExpenseItemId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<ExpenditureListDto>>;

public class GetExpendituresQueryHandler : IRequestHandler<GetExpendituresQuery, List<ExpenditureListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetExpendituresQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ExpenditureListDto>> Handle(GetExpendituresQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Expenditures
            .AsNoTracking()
            .Include(e => e.ExpenseItem)
            .Include(e => e.CreatedByUser)
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(e => e.ConcernID == targetConcernId.Value);
        }

        if (request.Type.HasValue && request.Type.Value > 0)
        {
            query = query.Where(e => e.ExpenseItem != null && e.ExpenseItem.Status == request.Type.Value);
        }

        if (request.ExpenseItemId.HasValue && request.ExpenseItemId.Value > 0)
        {
            query = query.Where(e => e.ExpenseItemID == request.ExpenseItemId.Value);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(e => e.EntryDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date;
            query = query.Where(e => e.EntryDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e =>
                (e.VoucherNo != null && e.VoucherNo.ToLower().Contains(search)) ||
                (e.Purpose != null && e.Purpose.ToLower().Contains(search)) ||
                (e.Remarks != null && e.Remarks.ToLower().Contains(search)) ||
                (e.ExpenseItem != null && e.ExpenseItem.Description.ToLower().Contains(search)));
        }

        return await query
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.ExpenditureID)
            .Select(e => new ExpenditureListDto
            {
                ExpenditureID = e.ExpenditureID,
                VoucherNo = e.VoucherNo,
                EntryDate = e.EntryDate,
                Amount = e.Amount,
                Purpose = e.Purpose,
                ExpenseItemID = e.ExpenseItemID,
                ExpenseItemName = e.ExpenseItem != null ? e.ExpenseItem.Description : null,
                ExpenseItemStatus = e.ExpenseItem != null ? e.ExpenseItem.Status : 1,
                ExpenseItemStatusName = e.ExpenseItem != null ? ((EnumCompanyTransaction)e.ExpenseItem.Status).ToString() : "Expense",
                WFStatus = e.WFStatus,
                WFStatusName = ((EnumWFStatus)e.WFStatus).ToString(),
                ConcernID = e.ConcernID,
                CreatedByName = e.CreatedByUser != null ? e.CreatedByUser.UserName : null
            })
            .ToListAsync(cancellationToken);
    }
}
