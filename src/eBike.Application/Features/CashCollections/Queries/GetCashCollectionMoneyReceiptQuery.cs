using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Helpers;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashCollections.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Queries;

public record GetCashCollectionMoneyReceiptQuery(int Id) : IRequest<CashCollectionMoneyReceiptDto>;

public class GetCashCollectionMoneyReceiptQueryHandler : IRequestHandler<GetCashCollectionMoneyReceiptQuery, CashCollectionMoneyReceiptDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetCashCollectionMoneyReceiptQueryHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<CashCollectionMoneyReceiptDto> Handle(GetCashCollectionMoneyReceiptQuery request, CancellationToken cancellationToken)
    {
        var collection = await _context.CashCollections
            .AsNoTracking()
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.CashCollectionID == request.Id, cancellationToken);

        if (collection == null)
        {
            throw new NotFoundException(nameof(CashCollection), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        var sysInfo = await connection.QueryFirstOrDefaultAsync<CashCollectionCompanyDto>(@"
            SELECT TOP 1 
                ConcernID,
                Name, 
                Address, 
                TelephoneNo, 
                EmailAddress, 
                WebAddress, 
                VATRegistrationNo
            FROM SystemInformations 
            WHERE ConcernID = @ConcernId", new { ConcernId = collection.ConcernID });

        if (sysInfo == null || string.IsNullOrWhiteSpace(sysInfo.Name))
        {
            var concern = await _context.SisterConcerns.AsNoTracking()
                .FirstOrDefaultAsync(sc => sc.ConcernID == collection.ConcernID, cancellationToken);

            sysInfo = new CashCollectionCompanyDto
            {
                ConcernID = collection.ConcernID,
                Name = concern?.Name ?? "Object Canvas Technology",
                Address = concern?.Address,
                TelephoneNo = concern?.ContactNo
            };
        }

        string? createdByName = null;
        if (collection.CreatedBy > 0)
        {
            createdByName = await connection.ExecuteScalarAsync<string?>(
                "SELECT UserName FROM Users WHERE Id = @UserId", new { UserId = collection.CreatedBy });
        }

        var customer = collection.Customer;
        var customerDto = new CashCollectionCustomerDto
        {
            CustomerID = collection.CustomerID ?? 0,
            CustomerCode = customer?.Code ?? string.Empty,
            CustomerName = customer?.Name ?? string.Empty,
            ContactNo = customer?.ContactNo,
            Address = customer?.Address,
            FatherName = customer?.FName,
            TotalDue = customer?.TotalDue ?? collection.BalanceDue
        };

        DateTime? lastSalesDate = null;
        if (collection.CustomerID.HasValue && collection.CustomerID.Value > 0)
        {
            lastSalesDate = await _context.SOrders
                .AsNoTracking()
                .Where(s => s.CustomerID == collection.CustomerID.Value)
                .OrderByDescending(s => s.InvoiceDate)
                .Select(s => (DateTime?)s.InvoiceDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        string projectTypeName = collection.ProjectType == (int)EnumProjectType.AutoParts ? "AutoParts" : "Motorcycle";

        var receiptMetadata = new CashCollectionReceiptMetadataDto
        {
            CashCollectionID = collection.CashCollectionID,
            ReceiptNo = collection.ReceiptNo ?? string.Empty,
            MRNo = collection.MRNo,
            ReceiptDate = collection.EntryDate,
            LastSalesDate = lastSalesDate,
            ProjectType = collection.ProjectType,
            ProjectTypeName = projectTypeName,
            Remarks = collection.Remarks,
            PrintDate = DateTime.Now,
            CreatedByID = collection.CreatedBy,
            CreatedByName = createdByName
        };

        decimal amount = collection.Amount ?? 0m;
        decimal adjustAmt = collection.AdjustAmt;
        decimal interestAmt = collection.InterestAmt;
        int tranType = collection.TransactionType ?? (int)EnumTranType.FromCustomer;

        decimal netCollectionAmount;
        if (tranType == (int)EnumTranType.CollectionReturn)
        {
            netCollectionAmount = -(amount + adjustAmt);
        }
        else
        {
            netCollectionAmount = amount + adjustAmt - interestAmt;
        }

        int payType = collection.PaymentType ?? (int)EnumPayType.Cash;
        string paymentTypeName = Enum.IsDefined(typeof(EnumPayType), payType)
            ? ((EnumPayType)payType).ToString()
            : payType.ToString();

        string tranTypeName = Enum.IsDefined(typeof(EnumTranType), tranType)
            ? ((EnumTranType)tranType).ToString()
            : tranType.ToString();

        var paymentDto = new CashCollectionPaymentDto
        {
            Amount = amount,
            AdjustAmt = adjustAmt,
            InterestAmt = interestAmt,
            NetCollectionAmount = netCollectionAmount,
            InWordsAmount = NumberToWordsConverter.ToTakaWords(amount),
            PaymentType = payType,
            PaymentTypeName = paymentTypeName,
            TransactionType = tranType,
            TransactionTypeName = tranTypeName,
            AccountNo = collection.AccountNo,
            BankName = collection.BankName,
            BranchName = collection.BranchName,
            MBAccountNo = collection.MBAccountNo,
            BKashNo = collection.BKashNo,
            BalanceDue = customer?.TotalDue ?? collection.BalanceDue
        };

        return new CashCollectionMoneyReceiptDto
        {
            Company = sysInfo,
            Customer = customerDto,
            Receipt = receiptMetadata,
            Payment = paymentDto
        };
    }
}

