using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Helpers;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetSalesOrderMoneyReceiptQuery(int OrderId) : IRequest<SalesOrderMoneyReceiptDto>;

public class GetSalesOrderMoneyReceiptQueryHandler : IRequestHandler<GetSalesOrderMoneyReceiptQuery, SalesOrderMoneyReceiptDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetSalesOrderMoneyReceiptQueryHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<SalesOrderMoneyReceiptDto> Handle(GetSalesOrderMoneyReceiptQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
                .ThenInclude(c => c!.Employee)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.Product)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.StockDetail)
            .FirstOrDefaultAsync(s => s.SOrderID == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(SOrder), request.OrderId);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        var sysInfo = await connection.QueryFirstOrDefaultAsync<InvoiceCompanyDto>(@"
            SELECT TOP 1 
                ConcernID,
                Name, 
                Address, 
                TelephoneNo, 
                EmailAddress, 
                WebAddress, 
                VATRegistrationNo
            FROM SystemInformations 
            WHERE ConcernID = @ConcernId", new { ConcernId = order.ConcernID });

        if (sysInfo == null || string.IsNullOrWhiteSpace(sysInfo.Name))
        {
            var concern = await _context.SisterConcerns.AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.ConcernID == order.ConcernID, cancellationToken);

            sysInfo = new InvoiceCompanyDto
            {
                ConcernID = order.ConcernID,
                Name = concern?.Name ?? "Showroom",
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

        string? bankName = null;
        if (order.CardPaidAmount > 0 && order.CardTypeSetupID > 0)
        {
            bankName = await connection.ExecuteScalarAsync<string?>(@"
                SELECT b.BankName 
                FROM CardTypeSetups cts 
                JOIN Banks b ON cts.BankID = b.BankID 
                WHERE cts.CardTypeSetupID = @SetupId", new { SetupId = order.CardTypeSetupID });
        }

        var customer = order.Customer;
        decimal totalDue = customer?.TotalDue ?? order.TotalDue;
        decimal previousDue = totalDue - order.PaymentDue;

        var customerDto = new MoneyReceiptCustomerDto
        {
            CustomerID = order.CustomerID,
            CustomerCode = customer?.Code ?? string.Empty,
            CustomerName = customer?.Name ?? string.Empty,
            ContactNo = customer?.ContactNo,
            Address = customer?.Address,
            FatherName = customer?.FName
        };

        DateTime? lastSalesDate = await _context.SOrders
            .AsNoTracking()
            .Where(s => s.CustomerID == order.CustomerID && s.SOrderID != order.SOrderID && s.InvoiceDate <= order.InvoiceDate)
            .OrderByDescending(s => s.InvoiceDate)
            .Select(s => (DateTime?)s.InvoiceDate)
            .FirstOrDefaultAsync(cancellationToken);

        string soTypeName = order.SOType switch
        {
            2 => "Auto Parts",
            3 => "Credit",
            _ => "Vehicle"
        };

        var receiptMetadata = new MoneyReceiptMetadataDto
        {
            SOrderID = order.SOrderID,
            ReceiptNo = !string.IsNullOrWhiteSpace(order.MRNo) ? order.MRNo : order.InvoiceNo,
            ReceiptDate = order.InvoiceDate,
            InvoiceNo = order.InvoiceNo,
            InvoiceDate = order.InvoiceDate,
            LastSalesDate = lastSalesDate,
            SOType = order.SOType,
            SOTypeName = soTypeName,
            Remarks = order.Remarks,
            PrintDate = DateTime.Now
        };

        var productNames = order.SOrderDetails
            .Where(d => d.Product != null && !string.IsNullOrWhiteSpace(d.Product.ProductName))
            .Select(d => d.Product!.ProductName)
            .Distinct();

        var chassisList = order.SOrderDetails
            .Where(d => d.StockDetail != null && !string.IsNullOrWhiteSpace(d.StockDetail.IMENO))
            .Select(d => d.StockDetail!.IMENO);

        var engineList = order.SOrderDetails
            .Where(d => d.StockDetail != null && !string.IsNullOrWhiteSpace(d.StockDetail.EngineNo))
            .Select(d => d.StockDetail!.EngineNo);

        var vehicleInfoDto = new MoneyReceiptVehicleInfoDto
        {
            ProductNames = string.Join(", ", productNames),
            ChassisNumbers = string.Join(", ", chassisList),
            EngineNumbers = string.Join(", ", engineList)
        };


        decimal recAmount = order.RecAmount ?? 0m;
        decimal cardAmount = order.CardPaidAmount;
        decimal cashAmount = Math.Max(0m, recAmount - cardAmount);

        string paymentMode = "Cash";
        if (cardAmount > 0 && cashAmount > 0)
        {
            paymentMode = "Cash + Card";
        }
        else if (cardAmount > 0)
        {
            paymentMode = "Card";
        }

        var paymentDto = new MoneyReceiptPaymentDto
        {
            ReceivedAmount = recAmount,
            InWordsAmount = NumberToWordsConverter.ToTakaWords(recAmount),
            PaymentMode = paymentMode,
            CashPaidAmount = cashAmount,
            CardPaidAmount = cardAmount,
            BankName = bankName,
            CurrentDue = order.PaymentDue,
            PreviousDue = previousDue,
            TotalDue = totalDue
        };

        var salesPersonDto = new InvoiceSalesPersonDto
        {
            EmployeeID = customer?.EmployeeID,
            EmployeeName = customer?.Employee?.Name,
            EmployeeContact = customer?.Employee?.ContactNo,
            CreatedByID = order.CreatedBy,
            CreatedByName = createdByName
        };

        return new SalesOrderMoneyReceiptDto
        {
            Company = sysInfo,
            Customer = customerDto,
            Receipt = receiptMetadata,
            Vehicle = vehicleInfoDto,
            Payment = paymentDto,
            SalesPerson = salesPersonDto
        };
    }
}
