using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseDOs.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseDOs.Queries;

public record GetPurchaseDOByIdQuery(int Id) : IRequest<PurchaseDODto?>;

public class GetPurchaseDOByIdQueryHandler : IRequestHandler<GetPurchaseDOByIdQuery, PurchaseDODto?>
{
    private readonly IApplicationDbContext _context;

    public GetPurchaseDOByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseDODto?> Handle(GetPurchaseDOByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.DOes
            .Include(d => d.Customer)
            .Include(d => d.Supplier)
            .Include(d => d.DODetails)
                .ThenInclude(det => det.Product)
            .Include(d => d.DODetails)
                .ThenInclude(det => det.Color)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DOID == request.Id, cancellationToken);

        if (entity == null) return null;

        return new PurchaseDODto
        {
            DOID = entity.DOID,
            Date = entity.Date,
            DONo = entity.DONo,
            CustomerID = entity.CustomerID,
            CustomerName = entity.Customer?.Name,
            SupplierID = entity.SupplierID,
            SupplierName = entity.Supplier?.Name,
            Status = entity.Status,
            Remarks = entity.Remarks,
            TotalAmt = entity.TotalAmt,
            PaidAmt = entity.PaidAmt,
            NetDiscount = entity.NetDiscount,
            TotalTax = entity.TotalTax,
            GrandTotal = entity.GrandTotal,
            ConcernID = entity.ConcernID,
            Details = entity.DODetails.Select(det => new PurchaseDODetailDto
            {
                DODID = det.DODID,
                DOID = det.DOID,
                ProductID = det.ProductID,
                ProductName = det.Product?.ProductName,
                ProductCode = det.Product?.Code,
                ColorID = det.ColorID,
                ColorName = det.Color?.Name,
                DOQty = det.DOQty,
                GivenQty = det.GivenQty,
                MRP = det.MRP,
                UnitPrice = det.UnitPrice,
                TotalAmt = det.TotalAmt,
                PPDisPercent = det.PPDisPercent,
                PPDisAmt = det.PPDisAmt,
                PPTaxAmount = det.PPTaxAmount
            }).ToList()
        };
    }
}
