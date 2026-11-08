using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Transfers.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Transfers.Queries;

public record GetTransferEligibleStockQuery(
    string? Search = null,
    int? GodownId = null,
    int? ConcernId = null
) : IRequest<List<TransferEligibleStockDto>>;

public class GetTransferEligibleStockQueryHandler : IRequestHandler<GetTransferEligibleStockQuery, List<TransferEligibleStockDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetTransferEligibleStockQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TransferEligibleStockDto>> Handle(GetTransferEligibleStockQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var query = from sd in _context.StockDetails.AsNoTracking()
                    join p in _context.Products.AsNoTracking() on sd.ProductID equals p.ProductID
                    join col in _context.Colors.AsNoTracking() on sd.ColorID equals col.ColorID into colGroup
                    from col in colGroup.DefaultIfEmpty()
                    join g in _context.Godowns.AsNoTracking() on sd.GodownID equals g.GodownID into gGroup
                    from g in gGroup.DefaultIfEmpty()
                    where sd.Status == (int)EnumStockStatus.Stock
                          && sd.Quantity > 0
                    select new { sd, p, col, g };

        if (targetConcernId > 0)
        {
            query = query.Where(x => x.p.ConcernID == targetConcernId);
        }

        if (request.GodownId.HasValue && request.GodownId.Value > 0)
        {
            query = query.Where(x => x.sd.GodownID == request.GodownId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.sd.IMENO.ToLower().Contains(search) ||
                x.sd.EngineNo.ToLower().Contains(search) ||
                x.p.ProductName.ToLower().Contains(search) ||
                x.p.Code.ToLower().Contains(search)
            );
        }

        var results = await query
            .OrderBy(x => x.p.ProductName)
            .Take(100)
            .Select(x => new TransferEligibleStockDto
            {
                ProductId = x.p.ProductID,
                ProductCode = x.p.Code,
                ProductName = x.p.ProductName,
                StockDetailId = x.sd.SDetailID,
                IMENO = x.sd.IMENO,
                EngineNo = x.sd.EngineNo,
                KeyNo = x.sd.KeyNo,
                ColorId = x.sd.ColorID,
                ColorName = x.col != null ? x.col.Name : null,
                GodownId = x.sd.GodownID,
                GodownName = x.g != null ? x.g.Name : null,
                PRate = x.sd.PRate > 0 ? x.sd.PRate : (x.p.MRP ?? 0),
                SRate = x.sd.SRate > 0 ? x.sd.SRate : (x.p.MRP ?? 0),
                Quantity = x.sd.Quantity
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
