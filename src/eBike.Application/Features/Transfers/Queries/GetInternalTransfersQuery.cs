using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Queries;

public record GetInternalTransfersQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? ConcernId = null,
    string? Search = null
) : IRequest<List<InternalTransferHistoryDto>>;

public class GetInternalTransfersQueryHandler : IRequestHandler<GetInternalTransfersQuery, List<InternalTransferHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetInternalTransfersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<InternalTransferHistoryDto>> Handle(GetInternalTransfersQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var query = _context.TransferHistories
            .AsNoTracking()
            .Include(h => h.Product)
            .Include(h => h.FromGodownNavigation)
            .Include(h => h.ToGodownNavigation)
            .AsQueryable();

        if (targetConcernId > 0)
        {
            query = query.Where(h => h.ConcernID == targetConcernId);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(h => h.TransferDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(h => h.TransferDate <= to);
        }

        var list = await query
            .OrderByDescending(h => h.TransferDate)
            .ThenByDescending(h => h.TransferHID)
            .Take(200)
            .ToListAsync(cancellationToken);

        var sDetailIds = list.Select(h => h.FromSDetailID).Where(id => id > 0).Distinct().ToList();
        var stockDetails = await _context.StockDetails
            .AsNoTracking()
            .Where(sd => sDetailIds.Contains(sd.SDetailID))
            .ToDictionaryAsync(sd => sd.SDetailID, cancellationToken);

        return list.Select(h =>
        {
            stockDetails.TryGetValue(h.FromSDetailID, out var stockDetail);

            return new InternalTransferHistoryDto
            {
                TransferHID = h.TransferHID,
                TransferDate = h.TransferDate,
                ProductId = h.ProductId,
                ProductCode = h.Product?.Code,
                ProductName = h.Product?.ProductName,
                FromGodownId = h.FromGodown,
                FromGodownName = h.FromGodownNavigation?.Name ?? $"Godown #{h.FromGodown}",
                ToGodownId = h.ToGodown,
                ToGodownName = h.ToGodownNavigation?.Name ?? $"Godown #{h.ToGodown}",
                Qty = h.Qty,
                IMENO = stockDetail?.IMENO,
                EngineNo = stockDetail?.EngineNo
            };
        }).ToList();
    }
}
