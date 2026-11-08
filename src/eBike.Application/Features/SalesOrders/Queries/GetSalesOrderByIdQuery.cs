using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetSalesOrderByIdQuery(int Id) : IRequest<SalesOrderDto>;

public class GetSalesOrderByIdQueryHandler : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalesOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesOrderDto> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.SOrders
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

        if (order == null)
        {
            throw new NotFoundException(nameof(SOrder), request.Id);
        }

        var dto = new SalesOrderDto
        {
            SOrderID = order.SOrderID,
            InvoiceNo = order.InvoiceNo,
            InvoiceDate = order.InvoiceDate,
            VATPercentage = order.VATPercentage,
            VATAmount = order.VATAmount,
            GrandTotal = order.GrandTotal,
            TDPercentage = order.TDPercentage,
            TDAmount = order.TDAmount,
            TotalAmount = order.TotalAmount,
            PaymentDue = order.PaymentDue,
            RecAmount = order.RecAmount,
            NetDiscount = order.NetDiscount,
            Remarks = order.Remarks,
            CustomerID = order.CustomerID,
            CustomerCode = order.Customer?.Code,
            CustomerName = order.Customer?.Name,
            ContactNo = order.Customer?.ContactNo,
            CustomerAddress = order.Customer?.Address,
            CustomerTotalDue = order.Customer?.TotalDue ?? 0,
            AdjAmount = order.AdjAmount,
            TotalDue = order.TotalDue,
            Status = order.Status,
            ConcernID = order.ConcernID,
            TotalOffer = order.TotalOffer,
            CardPaidAmount = order.CardPaidAmount,
            CardTypeSetupID = order.CardTypeSetupID,
            DepositChargePercent = order.DepositChargePercent,
            TotalMushok = order.TotalMushok,
            TotalInstAmt = order.TotalInstAmt,
            TOfferAmt = order.TOfferAmt,
            MRNo = order.MRNo,
            LedgerNo = order.LedgerNo,
            REGMRNo = order.REGMRNo,
            VATChallanPotroNo = order.VATChallanPotroNo,
            SOType = order.SOType,
            Details = order.SOrderDetails.Select(d => new SalesOrderDetailDto
            {
                SOrderDetailID = d.SOrderDetailID,
                ProductID = d.ProductID,
                ProductCode = d.Product?.Code,
                ProductName = d.Product?.ProductName,
                SDetailID = d.SDetailID,
                IMENO = d.StockDetail?.IMENO,
                EngineNo = d.StockDetail?.EngineNo,
                KeyNo = d.StockDetail?.KeyNo,
                ColorID = d.StockDetail?.ColorID,
                ColorName = d.StockDetail?.Color?.Name,
                GodownID = d.StockDetail?.GodownID ?? (int.TryParse(d.Compressor, out var gid) ? gid : 0),
                GodownName = d.StockDetail?.Godown?.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                PPDPercentage = d.PPDPercentage,
                PPDAmount = d.PPDAmount,
                UTAmount = d.UTAmount,
                MPRate = d.MPRate,
                PRate = d.PRate,
                SRate = d.SRate,
                PPOffer = d.PPOffer,
                Mushok = d.Mushok,
                SRateWMushok = d.SRateWMushok,
                RegType = d.RegType,
                RegAmount = d.RegAmount,
                InterestRate = d.InterestRate,
                InterestAmt = d.InterestAmt,
                InsuranceAmt = d.InsuranceAmt,
                Remarks = d.Remarks
            }).ToList()
        };

        return dto;
    }
}
