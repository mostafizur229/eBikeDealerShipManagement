using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Banks.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Banks.Queries;

public record GetBanksQuery(
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<BankListDto>>;

public class GetBanksQueryHandler : IRequestHandler<GetBanksQuery, List<BankListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetBanksQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BankListDto>> Handle(GetBanksQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Banks
            .AsNoTracking()
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(b => b.ConcernID == targetConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(b =>
                (b.BankName != null && b.BankName.ToLower().Contains(search)) ||
                (b.Code != null && b.Code.ToLower().Contains(search)) ||
                (b.AccountNo != null && b.AccountNo.ToLower().Contains(search)) ||
                (b.BranchName != null && b.BranchName.ToLower().Contains(search)) ||
                (b.AccountName != null && b.AccountName.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(b => b.Code)
            .Select(b => new BankListDto
            {
                BankID = b.BankID,
                Code = b.Code,
                BankName = b.BankName,
                AccountNo = b.AccountNo,
                BranchName = b.BranchName,
                AccountName = b.AccountName,
                OpeningBalance = b.OpeningBalance,
                TotalAmount = b.TotalAmount,
                IsAdvancedDueLimitApplicable = b.IsAdvancedDueLimitApplicable == 1,
                AdvancedAmountLimit = b.AdvancedAmountLimit ?? 0m,
                ConcernID = b.ConcernID
            })
            .ToListAsync(cancellationToken);
    }
}
