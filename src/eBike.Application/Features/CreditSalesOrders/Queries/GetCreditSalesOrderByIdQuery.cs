using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditSalesOrders.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditSalesOrders.Queries;

public record GetCreditSalesOrderByIdQuery(int Id) : IRequest<CreditSalesOrderDto>;

public class GetCreditSalesOrderByIdQueryHandler : IRequestHandler<GetCreditSalesOrderByIdQuery, CreditSalesOrderDto>
{
    private readonly IApplicationDbContext _context;

    public GetCreditSalesOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreditSalesOrderDto> Handle(GetCreditSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.CreditSales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.Product)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Color)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Godown)
            .Include(s => s.CreditSalesSchedules)
            .FirstOrDefaultAsync(s => s.CreditSalesID == request.Id, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(CreditSale), request.Id);
        }

        var dto = new CreditSalesOrderDto
        {
            CreditSalesID = order.CreditSalesID,
            InvoiceNo = order.InvoiceNo,
            SalesDate = order.SalesDate,
            IssueDate = order.IssueDate,
            CustomerID = order.CustomerID,
            CustomerCode = order.Customer?.Code,
            CustomerName = order.Customer?.Name,
            ContactNo = order.Customer?.ContactNo,
            CustomerAddress = order.Customer?.Address,
            CustomerCreditDue = order.Customer?.CreditDue ?? 0m,
            LedgerNo = order.LedgerNo,
            MRNo = order.MRNo,
            REGMRNo = order.REGMRNo,
            VATPercentage = order.VATPercentage,
            VATAmount = order.VATAmount,
            GrandTotal = order.TSalesAmt,
            Discount = order.Discount,
            NetAmount = order.NetAmount,
            DownPayment = order.DownPayment,
            Remaining = order.Remaining,
            InterestRate = order.InterestRate,
            InterestAmount = order.InterestAmount,
            TotalOffer = order.TotalOffer,
            TotalMushok = order.TotalMushok,
            NoOfInstallment = order.NoOfInstallment,
            IntallmentPrinciple = order.IntallmentPrinciple,
            InstallmentPeriod = order.InstallmentPeriod,
            LastPayAdjAmt = order.LastPayAdjAmt,
            Remarks = order.Remarks,
            Status = order.Status,
            ConcernID = order.ConcernID,
            CardPaidAmount = order.CardPaidAmount,
            CardTypeSetupID = order.CardTypeSetupID,
            DepositChargePercent = order.DepositChargePercent,
            BankTransID = order.BankTransID,
            CBankName = order.CBankName,
            CBBranch = order.CBBranch,
            CBChequeNo = order.CBChequeNo,
            CBNID = order.CBNID,
            VATChallanPotroNo = order.VATChallanPotroNo,
            Details = order.CreditSaleDetails.Select(d => new CreditSalesOrderDetailDto
            {
                CreditSaleDetailsID = d.CreditSaleDetailsID,
                ProductID = d.ProductID,
                ProductCode = d.Product?.Code,
                ProductName = d.Product?.ProductName,
                StockDetailID = d.StockDetailID,
                IMENO = d.StockDetail?.IMENO,
                EngineNo = d.StockDetail?.EngineNo,
                KeyNo = d.StockDetail?.KeyNo,
                ColorID = d.StockDetail?.ColorID ?? (d.ColorInfoId > 0 ? d.ColorInfoId : null),
                ColorName = d.StockDetail?.Color?.Name,
                GodownID = d.StockDetail?.GodownID ?? (int.TryParse(d.Compressor, out var gid) ? gid : null),
                GodownName = d.StockDetail?.Godown?.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                UTAmount = d.UTAmount,
                MPRate = d.MPRate,
                PRate = d.StockDetail?.PRate ?? 0m,
                SRate = d.SRate,
                PPOffer = d.PPOffer,
                IntPercentage = d.IntPercentage,
                IntTotalAmt = d.IntTotalAmt,
                Mushok = d.Mushok,
                SRateWMushok = d.SRateWMushok,
                RegType = int.TryParse(d.Motor, out var rt) ? rt : (d.RegAmt > 0 ? 1 : 0),
                RegAmount = d.RegAmt,
                InsuranceAmt = d.InsuranceAmt,
                OfferAmt = d.OfferAmt
            }).ToList(),
            Schedules = order.CreditSalesSchedules
                .OrderBy(sc => sc.ScheduleNo)
                .Select(sc => new CreditSalesScheduleDto
                {
                    CSScheduleID = sc.CSScheduleID,
                    ScheduleNo = sc.ScheduleNo,
                    MonthDate = sc.MonthDate,
                    PaymentDate = sc.PaymentDate > DateTime.MinValue.AddDays(1) && sc.PaymentStatus == "Paid" ? sc.PaymentDate : null,
                    Balance = sc.Balance,
                    InstallmentAmt = sc.InstallmentAmt,
                    ClosingBalance = sc.ClosingBalance,
                    PaymentStatus = sc.PaymentStatus,
                    ExpectedInstallment = sc.ExpectedInstallment,
                    HireValue = sc.HireValue,
                    NetValue = sc.NetValue,
                    LastPayAdjust = sc.LastPayAdjust,
                    MRNO = sc.MRNO,
                    Remarks = sc.Remarks,
                    RemindDate = sc.RemindDate,
                    CardPaidAmount = sc.CardPaidAmount,
                    CardTypeSetupID = sc.CardTypeSetupID,
                    DepositChargePercent = sc.DepositChargePercent
                }).ToList()
        };

        return dto;
    }
}
