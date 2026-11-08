using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Queries;

public record GetTransfersQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? ConcernId = null,
    string? Search = null
) : IRequest<List<TransferListDto>>;

public class GetTransfersQueryHandler : IRequestHandler<GetTransfersQuery, List<TransferListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetTransfersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TransferListDto>> Handle(GetTransfersQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var query = _context.Transfers
            .AsNoTracking()
            .Include(t => t.FromConcern)
            .Include(t => t.ToConcern)
            .Include(t => t.TransferDetails)
            .AsQueryable();

        if (targetConcernId > 0)
        {
            query = query.Where(t => t.FromConcernID == targetConcernId || t.ToConcernID == targetConcernId || t.ConcernID == targetConcernId);
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(t => t.TransferDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransferDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(t =>
                t.TransferNo.ToLower().Contains(search) ||
                (t.FromConcern != null && t.FromConcern.Name.ToLower().Contains(search)) ||
                (t.ToConcern != null && t.ToConcern.Name.ToLower().Contains(search)) ||
                (t.Remarks != null && t.Remarks.ToLower().Contains(search))
            );
        }

        var transfers = await query
            .OrderByDescending(t => t.TransferDate)
            .ThenByDescending(t => t.TransferID)
            .ToListAsync(cancellationToken);

        return transfers.Select(t => new TransferListDto
        {
            TransferId = t.TransferID,
            TransferNo = t.TransferNo,
            TransferDate = t.TransferDate,
            FromConcernId = t.FromConcernID,
            FromConcernName = t.FromConcern?.Name ?? $"Branch #{t.FromConcernID}",
            ToConcernId = t.ToConcernID,
            ToConcernName = t.ToConcern?.Name ?? $"Branch #{t.ToConcernID}",
            TotalAmount = t.TotalAmount,
            Remarks = t.Remarks,
            Status = t.Status,
            ItemCount = t.TransferDetails.Count
        }).ToList();
    }
}
