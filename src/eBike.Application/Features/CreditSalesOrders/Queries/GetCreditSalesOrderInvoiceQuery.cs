using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Helpers;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditSalesOrders.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditSalesOrders.Queries;

public record GetCreditSalesOrderInvoiceQuery(int OrderId, bool IsInvoice2 = false) : IRequest<CreditSalesOrderInvoiceDto>;

public class GetCreditSalesOrderInvoiceQueryHandler : IRequestHandler<GetCreditSalesOrderInvoiceQuery, CreditSalesOrderInvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetCreditSalesOrderInvoiceQueryHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<CreditSalesOrderInvoiceDto> Handle(GetCreditSalesOrderInvoiceQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.CreditSales
            .AsNoTracking()
            .Include(s => s.Customer)
                .ThenInclude(c => c!.District)
            .Include(s => s.Customer)
                .ThenInclude(c => c!.ThanaObj)
            .Include(s => s.Customer)
                .ThenInclude(c => c!.Employee)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p!.Category)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p!.Company)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Color)
            .Include(s => s.CreditSaleDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Godown)
            .Include(s => s.CreditSalesSchedules)
            .FirstOrDefaultAsync(s => s.CreditSalesID == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(CreditSale), request.OrderId);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        var sysInfo = await connection.QueryFirstOrDefaultAsync<CreditSalesCompanyDto>(@"
            SELECT TOP 1 
                ConcernID,
                Name, 
                Address, 
                TelephoneNo, 
                EmailAddress, 
                WebAddress, 
                VATRegistrationNo,
                IsNewSInvoiceEnable
            FROM SystemInformations 
            WHERE ConcernID = @ConcernId", new { ConcernId = order.ConcernID });

        if (sysInfo == null || string.IsNullOrWhiteSpace(sysInfo.Name))
        {
            var concern = await _context.SisterConcerns.AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.ConcernID == order.ConcernID, cancellationToken);

            sysInfo = new CreditSalesCompanyDto
            {
                ConcernID = order.ConcernID,
                Name = concern?.Name ?? "Object Canvas Technology",
                Address = concern?.Address,
                TelephoneNo = concern?.ContactNo
            };
        }

        string? createdByName = null;
        if (order.CreatedBy > 0)
        {
            createdByName = await connection.ExecuteScalarAsync<string?>(
                "SELECT UserName FROM Users WHERE Id = @UserId", new { UserId = order.CreatedBy });
        }


        var customer = order.Customer;
        var customerDto = new CreditSalesCustomerDto
        {
            CustomerID = order.CustomerID,
            CustomerCode = customer?.Code ?? string.Empty,
            CustomerName = customer?.Name ?? string.Empty,
            ContactNo = customer?.ContactNo,
            Address = customer?.Address,
            PermanentAddress = customer?.PermanentAddress,
            FatherName = customer?.FName,
            MotherName = customer?.MName,
            NID = customer?.NID,
            DistrictName = customer?.District?.Name ?? customer?.Dist,
            ThanaName = customer?.ThanaObj?.Name ?? customer?.Thana,
            PostOffice = customer?.Post,
            RefName = customer?.RefName,
            RefFather = customer?.RefFName,
            RefAddress = customer?.RefAddress,
            RefContact = customer?.RefContact,
            CreditDue = customer?.CreditDue ?? 0m,
            TotalDue = customer?.TotalDue ?? 0m
        };


        string statusName = Enum.IsDefined(typeof(EnumSalesType), order.Status)
            ? ((EnumSalesType)order.Status).ToString()
            : order.Status.ToString();

        string concernName = sysInfo.Name;
        int isNewSInvoiceEnable = sysInfo.IsNewSInvoiceEnable ?? 0;

        string primaryTemplateName;
        string primaryTemplatePath;

        if (isNewSInvoiceEnable == 1)
        {
            primaryTemplateName = "CreditSalesInvoice";
            primaryTemplatePath = "CreditSales\\CreditSalesInvoice.rdlc";
        }
        else if (HasName(concernName, "NEW PARK"))
        {
            primaryTemplateName = "RetNewPCreditInvoice";
            primaryTemplatePath = "NewParkInvoice\\RetNewPCreditInvoice.rdlc";
        }
        else
        {
            primaryTemplateName = "MaxInvoice";
            primaryTemplatePath = "CreditSales\\MaxInvoice.rdlc";
        }

        bool hasInvoice2 = false;
        string? invoice2TemplateName = null;
        string? invoice2TemplatePath = null;

        if (HasName(concernName, "KUSHTIA"))
        {
            hasInvoice2 = true;
            invoice2TemplateName = "KushtiaBajajInvoice";
            invoice2TemplatePath = "Sales\\KushtiaBajajInvoice.rdlc";
        }
        else if (HasName(concernName, "SALMAN MOTORS"))
        {
            hasInvoice2 = true;
            invoice2TemplateName = "SalmanInvoice";
            invoice2TemplatePath = "Sales\\SalmanInvoice.rdlc";
        }
        else if (HasName(concernName, "TAJ MOTORS"))
        {
            hasInvoice2 = true;
            invoice2TemplateName = "TajMotorsInvoice";
            invoice2TemplatePath = "Sales\\TajMotorsInvoice.rdlc";
        }

        string activeTemplateName = (request.IsInvoice2 && hasInvoice2) ? invoice2TemplateName! : primaryTemplateName;
        string activeTemplatePath = (request.IsInvoice2 && hasInvoice2) ? invoice2TemplatePath! : primaryTemplatePath;

        var metadataDto = new CreditSalesMetadataDto
        {
            CreditSalesID = order.CreditSalesID,
            InvoiceNo = order.InvoiceNo,
            SalesDate = order.SalesDate,
            IssueDate = order.IssueDate,
            ReportHeader = "Credit Invoice",
            MRNo = order.MRNo,
            LedgerNo = order.LedgerNo,
            REGMRNo = order.REGMRNo,
            Status = order.Status,
            StatusName = statusName,
            Remarks = order.Remarks,
            PrintDate = DateTime.Now,
            CreatedByID = order.CreatedBy,
            CreatedByName = createdByName,
            EmployeeName = customer?.Employee?.Name,
            BankName = order.CBankName,
            BranchName = order.CBBranch,
            ChequeNo = order.CBChequeNo,
            TemplateName = activeTemplateName,
            TemplatePath = activeTemplatePath,
            HasInvoice2 = hasInvoice2,
            Invoice2TemplateName = invoice2TemplateName,
            Invoice2TemplatePath = invoice2TemplatePath
        };

        decimal totalSales = order.TSalesAmt;
        decimal downPayment = order.DownPayment;
        decimal remaining = (order.Remaining > 0)
            ? order.Remaining
            : Math.Max(0m, (order.TSalesAmt - order.TotalOffer - downPayment));

        var financialsDto = new CreditSalesFinancialsDto
        {
            TotalSales = totalSales,
            TotalOffer = order.TotalOffer,
            Discount = order.Discount,
            DownPayment = downPayment,
            Remaining = remaining,
            VATPercentage = order.VATPercentage,
            VATAmount = order.VATAmount,
            InterestRate = order.InterestRate,
            InterestAmount = order.InterestAmount,
            NetAmount = order.NetAmount,
            NoOfInstallment = order.NoOfInstallment,
            InstallmentPrinciple = order.IntallmentPrinciple,
            InWordsTotal = NumberToWordsConverter.ToTakaWords(totalSales),
            InWordsDownpayment = NumberToWordsConverter.ToTakaWords(downPayment)
        };

        var schedules = order.CreditSalesSchedules
            .OrderBy(s => s.ScheduleNo)
            .Select(s => new CreditSalesInvoiceScheduleDto
            {
                ScheduleNo = s.ScheduleNo,
                ScheduleDate = s.MonthDate,
                PaymentDate = (s.PaymentStatus == "Paid" && s.PaymentDate != default) ? s.PaymentDate : null,
                OpeningBalance = s.Balance,
                InstallmentAmount = s.InstallmentAmt,
                ClosingBalance = s.ClosingBalance,
                Status = !string.IsNullOrWhiteSpace(s.PaymentStatus) ? s.PaymentStatus : "Due"
            })
            .ToList();

        int sl = 1;
        var items = new List<CreditSalesInvoiceItemDto>();

        foreach (var detail in order.CreditSaleDetails)
        {
            var product = detail.Product;
            var stockDetail = detail.StockDetail;

            decimal intAmt = detail.IntTotalAmt;
            decimal vatAmt = detail.PPVat > 0 ? detail.PPVat : order.VATAmount;
            decimal totalAmt = detail.UTAmount + intAmt + vatAmt;

            items.Add(new CreditSalesInvoiceItemDto
            {
                Sl = sl++,
                CreditSaleDetailsID = detail.CreditSaleDetailsID,
                ProductID = detail.ProductID,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.ProductName ?? string.Empty,
                CategoryName = product?.Category?.Description,
                CompanyName = product?.Company?.Name,
                ColorName = stockDetail?.Color?.Name,
                GodownName = stockDetail?.Godown?.Name,

                ChassisNo = stockDetail?.IMENO,
                EngineNo = stockDetail?.EngineNo,
                KeyNo = stockDetail?.KeyNo,
                CC = product?.CC,
                NoOfCylinder = product?.NoofCylinder,
                SeatingCapacity = product?.SeatingCapacity,
                VehicleSize = product?.VehicleSize,

                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                IntAmt = intAmt,
                VatAmt = vatAmt,
                TotalAmt = totalAmt,

                CompressorWarranty = product?.CompressorWarrentyMonth,
                MotorWarranty = product?.MotorWarrentyMonth,
                ServiceWarranty = product?.ServiceWarrentyMonth,
                SparePartsWarranty = product?.SparePartsWarrentyMonth
            });
        }

        return new CreditSalesOrderInvoiceDto
        {
            Company = sysInfo,
            Customer = customerDto,
            Order = metadataDto,
            Financials = financialsDto,
            Schedules = schedules,
            Items = items
        };
    }

    private static bool HasName(string concernName, params string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(concernName)) return false;
        foreach (var kw in keywords)
        {
            if (concernName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

