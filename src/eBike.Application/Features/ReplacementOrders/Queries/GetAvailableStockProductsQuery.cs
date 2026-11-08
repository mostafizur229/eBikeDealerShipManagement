using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ReplacementOrders.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Queries;

public record GetAvailableStockProductsQuery(
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<AvailableStockProductDto>>;

public class GetAvailableStockProductsQueryHandler : IRequestHandler<GetAvailableStockProductsQuery, List<AvailableStockProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetAvailableStockProductsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AvailableStockProductDto>> Handle(GetAvailableStockProductsQuery request, CancellationToken cancellationToken)
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
            .Take(50)
            .Select(x => new AvailableStockProductDto
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
                SRate = x.sd.SRate > 0 ? x.sd.SRate : (x.p.MRP ?? 0),
                MRPRate = x.sd.PRate > 0 ? x.sd.PRate : (x.p.MRP ?? 0),
                Quantity = x.sd.Quantity
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
