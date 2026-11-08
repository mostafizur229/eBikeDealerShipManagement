using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseReturns.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseReturns.Queries;

public record GetPurchaseReturnByIdQuery(int Id) : IRequest<PurchaseReturnDto?>;

public class GetPurchaseReturnByIdQueryHandler : IRequestHandler<GetPurchaseReturnByIdQuery, PurchaseReturnDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseReturnByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseReturnDto?> Handle(GetPurchaseReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await _context.POrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Product)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Color)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Godown)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.POProductDetails)
            .FirstOrDefaultAsync(p => p.POrderID == request.Id, cancellationToken);

        if (po == null) return null;

        var dto = new PurchaseReturnDto
        {
            POrderID = po.POrderID,
            OrderDate = po.OrderDate,
            ChallanNo = po.ChallanNo,
            SupplierID = po.SupplierID,
            SupplierCode = po.Supplier?.Code,
            SupplierName = po.Supplier?.Name,
            SupplierContact = po.Supplier?.ContactNo,
            SupplierTotalDue = po.Supplier?.TotalDue ?? 0,
            GrandTotal = po.GrandTotal,
            TotalAmt = po.TotalAmt,
            RecAmt = po.RecAmt,
            PaymentDue = po.PaymentDue,
            TotalDue = po.TotalDue,
            Status = po.Status,
            StatusName = "Return",
            Remarks = po.Remarks,
            ConcernID = po.ConcernID,
            Details = po.POrderDetails.Select(d => new PurchaseReturnDetailDto
            {
                POrderDetailID = d.POrderDetailID,
                ProductID = d.ProductID,
                ProductCode = d.Product?.Code,
                ProductName = d.Product?.ProductName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TAmount = d.TAmount,
                ColorID = d.ColorID,
                ColorName = d.Color?.Name,
                GodownID = d.GodownID,
                GodownName = d.Godown?.Name,
                Serials = d.POProductDetails.Select(s => new PurchaseReturnSerialDto
                {
                    POPDID = s.POPDID,
                    ProductID = s.ProductID,
                    ColorID = s.ColorID,
                    ColorName = d.Color?.Name,
                    IMENO = s.IMENO,
                    EngineNo = s.EngineNo,
                    KeyNo = s.KeyNo,
                    ReturnSDetailID = s.ReturnSDetailID,
                    ReturnQty = s.ReturnQty
                }).ToList()
            }).ToList()
        };

        return dto;
    }
}
