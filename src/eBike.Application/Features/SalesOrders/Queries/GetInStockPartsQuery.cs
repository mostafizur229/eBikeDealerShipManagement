using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetInStockPartsQuery(
    string? Search = null,
    int? ProductId = null,
    int? GodownId = null,
    int? Limit = 100,
    int? ConcernId = null
) : IRequest<List<InStockPartDto>>;

public class GetInStockPartsQueryHandler : IRequestHandler<GetInStockPartsQuery, List<InStockPartDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetInStockPartsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<InStockPartDto>> Handle(GetInStockPartsQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var baseQuery = _context.StockDetails
            .AsNoTracking()
            .Include(sd => sd.Product)
                .ThenInclude(p => p!.Category)
            .Include(sd => sd.Product)
                .ThenInclude(p => p!.Company)
            .Include(sd => sd.Stock)
            .Include(sd => sd.Color)
            .Include(sd => sd.Godown)
            .Where(sd => sd.Status == 1 &&
                         sd.Quantity > 0 &&
                         sd.Product != null &&
                         sd.Product.IsPartsService == 1 &&
                         (sd.Product.Category == null || (
                             !sd.Product.Category.Description.ToLower().Contains("motorcycle") &&
                             !sd.Product.Category.Description.ToLower().Contains("motor cycle") &&
                             !sd.Product.Category.Description.ToLower().Contains("bike")
                         )));

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            var concernFiltered = baseQuery.Where(sd => sd.Stock != null && sd.Stock.ConcernID == targetConcernId.Value);
            if (await concernFiltered.AnyAsync(cancellationToken))
            {
                baseQuery = concernFiltered;
            }
        }

        if (request.ProductId.HasValue && request.ProductId.Value > 0)
        {
            baseQuery = baseQuery.Where(sd => sd.ProductID == request.ProductId.Value);
        }

        if (request.GodownId.HasValue && request.GodownId.Value > 0)
        {
            baseQuery = baseQuery.Where(sd => sd.GodownID == request.GodownId.Value);
        }

        int maxLimit = request.Limit.HasValue && request.Limit.Value > 0 ? Math.Min(request.Limit.Value, 500) : 100;

        var query = from sd in baseQuery
                    join part in _context.Parts on sd.Product!.PartsNoID equals part.PartsNoID into partJ
                    from part in partJ.DefaultIfEmpty()
                    join loc in _context.PLocations on sd.Product!.PLocationID equals loc.PLocationID into locJ
                    from loc in locJ.DefaultIfEmpty()
                    select new
                    {
                        sd,
                        PartNo = part != null ? part.Description : null,
                        LocationName = loc != null ? loc.Description : null
                    };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.sd.Product != null && (
                    x.sd.Product.Code.ToLower().Contains(search) ||
                    x.sd.Product.ProductName.ToLower().Contains(search)
                )) ||
                (x.PartNo != null && x.PartNo.ToLower().Contains(search)) ||
                (x.LocationName != null && x.LocationName.ToLower().Contains(search)) ||
                (x.sd.Color != null && x.sd.Color.Name.ToLower().Contains(search))
            );
        }

        return await query
            .OrderBy(x => x.sd.Product != null ? x.sd.Product.ProductName : "")
            .ThenBy(x => x.sd.SDetailID)
            .Take(maxLimit)
            .Select(x => new InStockPartDto
            {
                SDetailID = x.sd.SDetailID,
                StockID = x.sd.StockID,
                ProductID = x.sd.ProductID,
                ProductCode = x.sd.Product != null ? x.sd.Product.Code : string.Empty,
                ProductName = x.sd.Product != null ? x.sd.Product.ProductName : string.Empty,
                PartsNoID = x.sd.Product != null ? x.sd.Product.PartsNoID : 0,
                PartNo = x.PartNo,
                PLocationID = x.sd.Product != null ? x.sd.Product.PLocationID : 0,
                LocationName = x.LocationName,
                CategoryID = x.sd.Product != null ? x.sd.Product.CategoryID : null,
                CategoryName = x.sd.Product != null && x.sd.Product.Category != null ? x.sd.Product.Category.Description : null,
                CompanyID = x.sd.Product != null ? x.sd.Product.CompanyID : null,
                CompanyName = x.sd.Product != null && x.sd.Product.Company != null ? x.sd.Product.Company.Name : null,
                ColorID = x.sd.ColorID,
                ColorName = x.sd.Color != null ? x.sd.Color.Name : null,
                GodownID = x.sd.GodownID,
                GodownName = x.sd.Godown != null ? x.sd.Godown.Name : null,
                AvailableQuantity = x.sd.Quantity,
                SalesRate = x.sd.SRate > 0 ? x.sd.SRate : (x.sd.Product != null && x.sd.Product.MRP.HasValue ? x.sd.Product.MRP.Value : 0m),
                MRPRate = x.sd.Product != null && x.sd.Product.MRP.HasValue ? x.sd.Product.MRP.Value : (x.sd.SRate > 0 ? x.sd.SRate : 0m),
                PRate = x.sd.PRate,
                PWDiscount = x.sd.Product != null ? x.sd.Product.PWDiscount : 0m,
                Mushok = x.sd.Product != null ? (x.sd.Product.PMushokRate + x.sd.Product.VatAmount) : 0m,
                UnitType = x.sd.Product != null && x.sd.Product.UnitType.HasValue ? x.sd.Product.UnitType.Value.ToString() : null
            })
            .ToListAsync(cancellationToken);
    }
}
