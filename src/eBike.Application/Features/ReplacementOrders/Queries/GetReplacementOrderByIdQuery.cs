using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ReplacementOrders.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Queries;

public record GetReplacementOrderByIdQuery(int Id) : IRequest<ReplacementOrderDto>;

public class GetReplacementOrderByIdQueryHandler : IRequestHandler<GetReplacementOrderByIdQuery, ReplacementOrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetReplacementOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReplacementOrderDto> Handle(GetReplacementOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SOrderDetails)
            .FirstOrDefaultAsync(s => s.SOrderID == request.Id && s.IsReplacement == 1, cancellationToken);

        if (order == null)
            throw new NotFoundException($"Replacement order with ID {request.Id} was not found.");

        // Fetch original damage items linked to this replacement order
        var damageItems = await _context.SOrderDetails
            .AsNoTracking()
            .Where(d => d.RepOrderID == order.SOrderID)
            .ToListAsync(cancellationToken);

        // Collect all stock detail IDs involved (both damage and new)
        var damageStockDetailIds = damageItems.Select(d => d.SDetailID).Distinct().ToList();
        var newStockDetailIds = order.SOrderDetails.Select(d => d.SDetailID).Distinct().ToList();
        var allStockDetailIds = damageStockDetailIds.Concat(newStockDetailIds).Distinct().ToList();

        var stockDetails = await _context.StockDetails
            .AsNoTracking()
            .Include(sd => sd.Product)
            .Include(sd => sd.Color)
            .Where(sd => allStockDetailIds.Contains(sd.SDetailID))
            .ToDictionaryAsync(sd => sd.SDetailID, cancellationToken);

        var pairedItems = new List<ReplacementOrderDetailDto>();

        foreach (var newItem in order.SOrderDetails)
        {
            stockDetails.TryGetValue(newItem.SDetailID, out var newStock);

            // Find matching damage item by RStockDetailId
            var damageItem = damageItems.FirstOrDefault(d => d.RStockDetailId == newItem.SDetailID);
            StockDetail? damageStock = null;
            if (damageItem != null && stockDetails.TryGetValue(damageItem.SDetailID, out var dStock))
            {
                damageStock = dStock;
            }

            pairedItems.Add(new ReplacementOrderDetailDto
            {
                SOrderDetailId = newItem.SOrderDetailID,
                SOrderId = order.SOrderID,
                DamageProductId = damageItem?.ProductID ?? damageStock?.ProductID ?? 0,
                DamageProductCode = damageStock?.Product?.Code,
                DamageProductName = damageStock?.Product?.ProductName,
                DamageStockDetailId = damageItem?.SDetailID ?? 0,
                DamageIMENO = damageStock?.IMENO,
                DamageEngineNo = damageStock?.EngineNo,
                DamageColorName = damageStock?.Color?.Name,
                DamageUnitPrice = damageItem?.UnitPrice ?? 0,
                ReplaceProductType = damageItem?.ReplaceProductType ?? 1,
                NewProductId = newItem.ProductID,
                NewProductCode = newStock?.Product?.Code,
                NewProductName = newStock?.Product?.ProductName,
                NewStockDetailId = newItem.SDetailID,
                NewIMENO = newStock?.IMENO,
                NewEngineNo = newStock?.EngineNo,
                NewColorName = newStock?.Color?.Name,
                NewUnitPrice = newItem.UnitPrice,
                Quantity = newItem.Quantity,
                Remarks = newItem.Remarks ?? damageItem?.Remarks
            });
        }

        decimal damageTotal = pairedItems.Sum(p => p.DamageUnitPrice);
        decimal replaceTotal = pairedItems.Sum(p => p.NewUnitPrice * p.Quantity);

        return new ReplacementOrderDto
        {
            SOrderId = order.SOrderID,
            InvoiceNo = order.InvoiceNo,
            InvoiceDate = order.InvoiceDate,
            CustomerId = order.CustomerID,
            CustomerName = order.Customer != null ? order.Customer.Name : "N/A",
            ContactNo = order.Customer?.ContactNo,
            CustomerAddress = order.Customer?.Address,
            DamageTotalAmount = damageTotal,
            ReplaceTotalAmount = replaceTotal,
            GrandTotal = order.GrandTotal != 0 ? order.GrandTotal : (replaceTotal - damageTotal),
            AdjAmount = order.AdjAmount,
            TotalAmount = order.TotalAmount,
            RecAmount = order.RecAmount ?? 0,
            PaymentDue = order.PaymentDue,
            TotalDue = order.Customer != null ? order.Customer.TotalDue : 0,
            Remarks = order.Remarks,
            CreateDate = order.CreateDate,
            Items = pairedItems
        };
    }
}
