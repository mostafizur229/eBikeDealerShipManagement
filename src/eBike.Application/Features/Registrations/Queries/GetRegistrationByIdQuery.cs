using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Domain.Entities;
using MediatR;

namespace eBike.Application.Features.Registrations.Queries;

public record GetRegistrationByIdQuery(int Id) : IRequest<RegistrationDto>;

public class GetRegistrationByIdQueryHandler : IRequestHandler<GetRegistrationByIdQuery, RegistrationDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetRegistrationByIdQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<RegistrationDto> Handle(GetRegistrationByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                r.RegistrationID,
                r.RegApplyDate,
                r.CustomerID,
                c.Name AS CustomerName,
                c.Code AS CustomerCode,
                c.ContactNo AS CustomerPhone,
                c.Address AS CustomerAddress,
                r.ProductID,
                p.ProductName,
                p.Code AS ProductCode,
                p.CC,
                COALESCE(sdCash.SDetailID, sdCredit.SDetailID) AS StockDetailID,
                COALESCE(sdCash.IMENO, sdCredit.IMENO) AS ChassisNo,
                COALESCE(sdCash.EngineNo, sdCredit.EngineNo) AS EngineNo,
                COALESCE(sdCash.KeyNo, sdCredit.KeyNo) AS KeyNo,
                COALESCE(clrCash.Name, clrCredit.Name) AS ColorName,
                r.SorderDetailID,
                r.CreditSaleDetailsID,
                CASE 
                    WHEN r.SorderDetailID IS NOT NULL AND r.SorderDetailID > 0 THEN 'Cash'
                    WHEN r.CreditSaleDetailsID IS NOT NULL AND r.CreditSaleDetailsID > 0 THEN 'Credit'
                    ELSE 'Direct'
                END AS SaleType,
                COALESCE(so.InvoiceNo, cs.InvoiceNo) AS InvoiceNo,
                COALESCE(r.SalesDate, so.InvoiceDate, cs.SalesDate) AS SalesDate,
                r.RegType,
                CASE r.RegType
                    WHEN 1 THEN '2 years'
                    WHEN 2 THEN '10 years'
                    WHEN 3 THEN 'BRTA'
                    ELSE 'None'
                END AS RegTypeName,
                r.RegFee,
                r.InsuranceAmt,
                r.REGMRNo,
                COALESCE(r.Iscompleted, 1) AS Iscompleted,
                CASE COALESCE(r.Iscompleted, 1)
                    WHEN 1 THEN 'Initiate'
                    WHEN 2 THEN 'Observation'
                    WHEN 3 THEN 'FileProcessing'
                    WHEN 4 THEN 'Completed'
                    ELSE 'Initiate'
                END AS StatusName,
                r.ObservationDate,
                r.BRTARegFee,
                r.BRTATIPS,
                r.FileReceivedDate,
                r.CompletionDate,
                r.RegNo,
                r.IsDocTransfer,
                r.NewCusID,
                newC.Name AS NewCustomerName,
                newC.Code AS NewCustomerCode,
                r.TransferDate,
                r.ConcernID,
                r.WFStatus,
                r.CreateDate,
                r.CreatedBy,
                r.ModifiedDate,
                r.ModifiedBy
            FROM Registrations r
            INNER JOIN Customers c ON r.CustomerID = c.CustomerID
            INNER JOIN Products p ON r.ProductID = p.ProductID
            LEFT JOIN SOrderDetails sod ON r.SorderDetailID = sod.SOrderDetailID
            LEFT JOIN SOrders so ON sod.SOrderID = so.SOrderID
            LEFT JOIN StockDetails sdCash ON COALESCE(sod.RStockDetailId, sod.SDetailID) = sdCash.SDetailID
            LEFT JOIN Colors clrCash ON sdCash.ColorID = clrCash.ColorID

            LEFT JOIN CreditSaleDetails csd ON r.CreditSaleDetailsID = csd.CreditSaleDetailsID
            LEFT JOIN CreditSales cs ON csd.CreditSalesID = cs.CreditSalesID
            LEFT JOIN StockDetails sdCredit ON csd.StockDetailID = sdCredit.SDetailID
            LEFT JOIN Colors clrCredit ON sdCredit.ColorID = clrCredit.ColorID

            LEFT JOIN Customers newC ON r.NewCusID = newC.CustomerID
            WHERE r.RegistrationID = @Id";

        var reg = await connection.QueryFirstOrDefaultAsync<RegistrationDto>(sql, new { Id = request.Id });
        if (reg == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        reg.ParsedCC = ParseCC(reg.CC);
        return reg;
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

