using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetInStockVehiclesQuery(
    string? Search = null,
    int? ProductId = null,
    int? GodownId = null,
    int? Limit = 100,
    int? ConcernId = null,
    bool ForCreditSales = false
) : IRequest<List<InStockVehicleDto>>;

public class GetInStockVehiclesQueryHandler : IRequestHandler<GetInStockVehiclesQuery, List<InStockVehicleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetInStockVehiclesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<InStockVehicleDto>> Handle(GetInStockVehiclesQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var query = _context.StockDetails
            .AsNoTracking()
            .Include(sd => sd.Product)
                .ThenInclude(p => p!.Category)
            .Include(sd => sd.Product)
                .ThenInclude(p => p!.Company)
            .Include(sd => sd.Stock)
            .Include(sd => sd.Color)
            .Include(sd => sd.Godown)
            .Where(sd => sd.Status == 1 && // 1 = In Stock
                         sd.Stock != null && sd.Stock.ConcernID == targetConcernId &&
                         sd.Product != null && sd.Product.IsPartsService == 0); // Vehicle only

        if (request.ProductId.HasValue && request.ProductId.Value > 0)
        {
            query = query.Where(sd => sd.ProductID == request.ProductId.Value);
        }

        if (request.GodownId.HasValue && request.GodownId.Value > 0)
        {
            query = query.Where(sd => sd.GodownID == request.GodownId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(sd =>
                sd.IMENO.ToLower().Contains(search) ||
                sd.EngineNo.ToLower().Contains(search) ||
                (sd.Product != null && (
                    sd.Product.Code.ToLower().Contains(search) ||
                    sd.Product.ProductName.ToLower().Contains(search)
                )) ||
                (sd.Color != null && sd.Color.Name.ToLower().Contains(search))
            );
        }

        int maxLimit = request.Limit.HasValue && request.Limit.Value > 0 ? Math.Min(request.Limit.Value, 500) : 100;

        return await query
            .OrderBy(sd => sd.Product != null ? sd.Product.ProductName : "")
            .ThenBy(sd => sd.IMENO)
            .Take(maxLimit)
            .Select(sd => new InStockVehicleDto
            {
                SDetailID = sd.SDetailID,
                ProductID = sd.ProductID,
                ProductCode = sd.Product != null ? sd.Product.Code : string.Empty,
                ProductName = sd.Product != null ? sd.Product.ProductName : string.Empty,
                CategoryName = sd.Product != null && sd.Product.Category != null ? sd.Product.Category.Description : null,
                CompanyName = sd.Product != null && sd.Product.Company != null ? sd.Product.Company.Name : null,
                ColorID = sd.ColorID,
                ColorName = sd.Color != null ? sd.Color.Name : null,
                GodownID = sd.GodownID,
                GodownName = sd.Godown != null ? sd.Godown.Name : null,
                IMENO = sd.IMENO,
                EngineNo = sd.EngineNo,
                KeyNo = sd.KeyNo,
                MRPRate = request.ForCreditSales
                    ? (sd.CreditSRate > 0 ? sd.CreditSRate : (sd.SRate > 0 ? sd.SRate : (sd.Product != null && sd.Product.MRP.HasValue ? sd.Product.MRP.Value : 0m)))
                    : (sd.SRate > 0 ? sd.SRate : (sd.Product != null && sd.Product.MRP.HasValue ? sd.Product.MRP.Value : 0m)),
                PRate = sd.PRate,
                SalesRate = request.ForCreditSales
                    ? (sd.CreditSRate > 0 ? sd.CreditSRate : (sd.SRate > 0 ? sd.SRate : (sd.Product != null && sd.Product.MRP.HasValue ? sd.Product.MRP.Value : 0m)))
                    : (sd.SRate > 0 ? sd.SRate : (sd.Product != null && sd.Product.MRP.HasValue ? sd.Product.MRP.Value : 0m)),
                CreditSalesRate = sd.CreditSRate > 0 ? sd.CreditSRate : (sd.SRate > 0 ? sd.SRate : (sd.Product != null && sd.Product.MRP.HasValue ? sd.Product.MRP.Value : 0m)),
                CRSalesRate12Month = sd.CRSalesRate12Month,
                CRSalesRate3Month = sd.CRSalesRate3Month,
                Mushok = sd.Product != null ? (sd.Product.PMushokRate + sd.Product.VatAmount) : 0m,
                SRateWMushok = sd.Product != null ? ((sd.Product.MRP ?? 0m) - (sd.Product.PMushokRate + sd.Product.VatAmount)) : 0m,
                StockQuantity = sd.Quantity > 0 ? sd.Quantity : 1m
            })
            .ToListAsync(cancellationToken);
    }
}
