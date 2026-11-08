using System.Globalization;
using System.Text.RegularExpressions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Queries;

public record GetCustomerUnregisteredBikesQuery(int CustomerId) : IRequest<List<CustomerUnregisteredBikeDto>>;

public class GetCustomerUnregisteredBikesQueryHandler : IRequestHandler<GetCustomerUnregisteredBikesQuery, List<CustomerUnregisteredBikeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerUnregisteredBikesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerUnregisteredBikeDto>> Handle(GetCustomerUnregisteredBikesQuery request, CancellationToken cancellationToken)
    {
        var registeredSOrderIds = await _context.Registrations
            .Where(r => r.SorderDetailID.HasValue)
            .Select(r => r.SorderDetailID!.Value)
            .ToListAsync(cancellationToken);

        var registeredCreditOrderIds = await _context.Registrations
            .Where(r => r.CreditSaleDetailsID.HasValue)
            .Select(r => r.CreditSaleDetailsID!.Value)
            .ToListAsync(cancellationToken);

        var result = new List<CustomerUnregisteredBikeDto>();

        var cashItems = await (
            from so in _context.SOrders
            where so.CustomerID == request.CustomerId && so.Status == 1
            join sod in _context.SOrderDetails on so.SOrderID equals sod.SOrderID
            where sod.IsProductReturn == 0 && !registeredSOrderIds.Contains(sod.SOrderDetailID)
            join p in _context.Products on sod.ProductID equals p.ProductID
            join sd in _context.StockDetails on (sod.RStockDetailId ?? sod.SDetailID) equals sd.SDetailID
            join c in _context.Customers on so.CustomerID equals c.CustomerID
            join clr in _context.Colors on sd.ColorID equals clr.ColorID into clrs
            from clr in clrs.DefaultIfEmpty()
            select new
            {
                StockDetailId = sd.SDetailID,
                ProductId = p.ProductID,
                ProductName = p.ProductName,
                ProductCode = p.Code,
                CC = p.CC,
                ChassisNo = sd.IMENO,
                EngineNo = sd.EngineNo,
                KeyNo = sd.KeyNo,
                ColorId = clr != null ? clr.ColorID : (int?)null,
                ColorName = clr != null ? clr.Name : null,
                UnitPrice = sod.MPRate,
                SalesDate = so.InvoiceDate,
                InvoiceNo = so.InvoiceNo,
                SOrderDetailId = sod.SOrderDetailID,
                CustomerId = c.CustomerID,
                CustomerName = c.Name,
                CustomerCode = c.Code
            }
        ).ToListAsync(cancellationToken);

        foreach (var item in cashItems)
        {
            result.Add(new CustomerUnregisteredBikeDto
            {
                StockDetailId = item.StockDetailId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductCode = item.ProductCode,
                CC = item.CC,
                ParsedCC = ParseCC(item.CC),
                ChassisNo = item.ChassisNo,
                EngineNo = item.EngineNo,
                KeyNo = item.KeyNo,
                ColorId = item.ColorId,
                ColorName = item.ColorName,
                UnitPrice = item.UnitPrice,
                SalesDate = item.SalesDate,
                InvoiceNo = item.InvoiceNo,
                SaleType = "Cash",
                SOrderDetailId = item.SOrderDetailId,
                CreditSaleDetailsId = null,
                CustomerId = item.CustomerId,
                CustomerName = item.CustomerName,
                CustomerCode = item.CustomerCode
            });
        }

        var creditItems = await (
            from cs in _context.CreditSales
            where cs.CustomerID == request.CustomerId && cs.IsStatus == 1 && cs.IsReturn == 0
            join csd in _context.CreditSaleDetails on cs.CreditSalesID equals csd.CreditSalesID
            where csd.IsProductReturn == 0 && !registeredCreditOrderIds.Contains(csd.CreditSaleDetailsID)
            join p in _context.Products on csd.ProductID equals p.ProductID
            join sd in _context.StockDetails on csd.StockDetailID equals sd.SDetailID
            join c in _context.Customers on cs.CustomerID equals c.CustomerID
            join clr in _context.Colors on sd.ColorID equals clr.ColorID into clrs
            from clr in clrs.DefaultIfEmpty()
            select new
            {
                StockDetailId = sd.SDetailID,
                ProductId = p.ProductID,
                ProductName = p.ProductName,
                ProductCode = p.Code,
                CC = p.CC,
                ChassisNo = sd.IMENO,
                EngineNo = sd.EngineNo,
                KeyNo = sd.KeyNo,
                ColorId = clr != null ? clr.ColorID : (int?)null,
                ColorName = clr != null ? clr.Name : null,
                UnitPrice = csd.UnitPrice,
                SalesDate = cs.SalesDate,
                InvoiceNo = cs.InvoiceNo,
                CreditSaleDetailsId = csd.CreditSaleDetailsID,
                CustomerId = c.CustomerID,
                CustomerName = c.Name,
                CustomerCode = c.Code
            }
        ).ToListAsync(cancellationToken);

        foreach (var item in creditItems)
        {
            result.Add(new CustomerUnregisteredBikeDto
            {
                StockDetailId = item.StockDetailId,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductCode = item.ProductCode,
                CC = item.CC,
                ParsedCC = ParseCC(item.CC),
                ChassisNo = item.ChassisNo,
                EngineNo = item.EngineNo,
                KeyNo = item.KeyNo,
                ColorId = item.ColorId,
                ColorName = item.ColorName,
                UnitPrice = item.UnitPrice,
                SalesDate = item.SalesDate,
                InvoiceNo = item.InvoiceNo,
                SaleType = "Credit",
                SOrderDetailId = null,
                CreditSaleDetailsId = item.CreditSaleDetailsId,
                CustomerId = item.CustomerId,
                CustomerName = item.CustomerName,
                CustomerCode = item.CustomerCode
            });
        }

        return result;
    }

    private static decimal ParseCC(string? ccStr)
    {
        if (string.IsNullOrWhiteSpace(ccStr)) return 0;
        var match = Regex.Match(ccStr, @"\d+(\.\d+)?");
        if (match.Success && decimal.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var cc))
        {
            return cc;
        }
        return 0;
    }
}
