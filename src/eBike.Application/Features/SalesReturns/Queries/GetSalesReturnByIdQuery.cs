using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesReturns.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesReturns.Queries;

public record GetSalesReturnByIdQuery(int Id) : IRequest<SalesReturnDto?>;

public class GetSalesReturnByIdQueryHandler : IRequestHandler<GetSalesReturnByIdQuery, SalesReturnDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSalesReturnByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReturnDto?> Handle(GetSalesReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var so = await _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.Product)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Color)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Godown)
            .FirstOrDefaultAsync(s => s.SOrderID == request.Id, cancellationToken);

        if (so == null) return null;

        var dto = new SalesReturnDto
        {
            SOrderID = so.SOrderID,
            InvoiceNo = so.InvoiceNo,
            InvoiceDate = so.InvoiceDate,
            CustomerID = so.CustomerID,
            CustomerName = so.Customer?.Name,
            CustomerContact = so.Customer?.ContactNo,
            CustomerAddress = so.Customer?.Address,
            CustomerTotalDue = so.Customer?.TotalDue ?? 0,
            GrandTotal = so.GrandTotal,
            TotalAmount = so.TotalAmount,
            RecAmount = so.RecAmount,
            PaymentDue = so.PaymentDue,
            TotalDue = so.TotalDue,
            Status = so.Status,
            StatusName = "Sales Return",
            Remarks = so.Remarks,
            ConcernID = so.ConcernID,
            Details = so.SOrderDetails.Select(d => new SalesReturnDetailDto
            {
                SOrderDetailID = d.SOrderDetailID,
                ProductID = d.ProductID,
                ProductCode = d.Product?.Code,
                ProductName = d.Product?.ProductName,
                StockDetailID = d.SDetailID > 0 ? d.SDetailID : null,
                IMENO = d.StockDetail?.IMENO,
                EngineNo = d.StockDetail?.EngineNo,
                KeyNo = d.StockDetail?.KeyNo,
                ColorID = d.StockDetail?.ColorID,
                ColorName = d.StockDetail?.Color?.Name,
                GodownID = d.StockDetail?.GodownID ?? 0,
                GodownName = d.StockDetail?.Godown?.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                UTAmount = d.UTAmount
            }).ToList()
        };

        return dto;
    }
}
