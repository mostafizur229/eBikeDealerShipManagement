using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ReplacementOrders.DTOs;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Queries;

public record GetCustomerEligibleProductsQuery(
    int CustomerId,
    string? Imei = null,
    int? ConcernId = null
) : IRequest<List<CustomerEligibleProductDto>>;

public class GetCustomerEligibleProductsQueryHandler : IRequestHandler<GetCustomerEligibleProductsQuery, List<CustomerEligibleProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCustomerEligibleProductsQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CustomerEligibleProductDto>> Handle(GetCustomerEligibleProductsQuery request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;

        var query = from so in _context.SOrders.AsNoTracking()
                    join sod in _context.SOrderDetails.AsNoTracking() on so.SOrderID equals sod.SOrderID
                    join sd in _context.StockDetails.AsNoTracking() on sod.SDetailID equals sd.SDetailID
                    join p in _context.Products.AsNoTracking() on sod.ProductID equals p.ProductID into pGroup
                    from p in pGroup.DefaultIfEmpty()
                    join col in _context.Colors.AsNoTracking() on sd.ColorID equals col.ColorID into colGroup
                    from col in colGroup.DefaultIfEmpty()
                    join g in _context.Godowns.AsNoTracking() on sd.GodownID equals g.GodownID into gGroup
                    from g in gGroup.DefaultIfEmpty()
                    where so.CustomerID == request.CustomerId
                          && so.Status == (int)EnumSalesType.Sales
                          && sd.Status == (int)EnumStockStatus.Sold
                          && sod.IsProductReturn != 1
                          && sod.RepOrderID == null
                    select new { so, sod, sd, p, col, g };

        if (!string.IsNullOrWhiteSpace(request.Imei))
        {
            var imei = request.Imei.Trim().ToLower();
            query = query.Where(x => x.sd.IMENO.ToLower().Contains(imei) || x.sd.EngineNo.ToLower().Contains(imei));
        }

        var results = await query
            .OrderByDescending(x => x.so.InvoiceDate)
            .Select(x => new CustomerEligibleProductDto
            {
                ProductId = x.sod.ProductID,
                ProductCode = x.p != null ? x.p.Code : null,
                ProductName = x.p != null ? x.p.ProductName : null,
                StockDetailId = x.sd.SDetailID,
                IMENO = x.sd.IMENO,
                EngineNo = x.sd.EngineNo,
                KeyNo = x.sd.KeyNo,
                ColorId = x.sd.ColorID,
                ColorName = x.col != null ? x.col.Name : null,
                GodownId = x.sd.GodownID,
                GodownName = x.g != null ? x.g.Name : null,
                SoldUnitPrice = x.sod.UnitPrice,
                SoldQuantity = x.sod.Quantity,
                InvoiceNo = x.so.InvoiceNo,
                InvoiceDate = x.so.InvoiceDate
            })
            .ToListAsync(cancellationToken);

        return results;
    }
}
