using Dapper;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using MediatR;

namespace eBike.Application.Features.Registrations.Queries;

public record GetRegistrationsQuery(
    int? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? CustomerId = null,
    string? Search = null,
    int? ConcernId = null
) : IRequest<List<RegistrationListDto>>;

public class GetRegistrationsQueryHandler : IRequestHandler<GetRegistrationsQuery, List<RegistrationListDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUser _currentUser;

    public GetRegistrationsQueryHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUser currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<List<RegistrationListDto>> Handle(GetRegistrationsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var sql = @"
            SELECT 
                r.RegistrationID,
                r.RegApplyDate,
                r.CustomerID,
                c.Name AS CustomerName,
                c.Code AS CustomerCode,
                c.ContactNo AS CustomerPhone,
                r.ProductID,
                p.ProductName,
                COALESCE(sdCash.IMENO, sdCredit.IMENO) AS ChassisNo,
                COALESCE(sdCash.EngineNo, sdCredit.EngineNo) AS EngineNo,
                COALESCE(sdCash.KeyNo, sdCredit.KeyNo) AS KeyNo,
                COALESCE(clrCash.Name, clrCredit.Name) AS ColorName,
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
                r.TransferDate,
                r.ConcernID
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
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (request.Status.HasValue && request.Status.Value > 0)
        {
            sql += " AND COALESCE(r.Iscompleted, 1) = @Status";
            parameters.Add("Status", request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            sql += " AND r.RegApplyDate >= @FromDate";
            parameters.Add("FromDate", request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            sql += " AND r.RegApplyDate < @ToDate";
            parameters.Add("ToDate", request.ToDate.Value.Date.AddDays(1));
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            sql += " AND r.CustomerID = @CustomerId";
            parameters.Add("CustomerId", request.CustomerId.Value);
        }

        if (targetConcernId.HasValue && targetConcernId.Value > 0)
        {
            sql += " AND r.ConcernID = @ConcernId";
            parameters.Add("ConcernId", targetConcernId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            sql += @" AND (
                c.Name LIKE @SearchPattern OR 
                c.Code LIKE @SearchPattern OR 
                c.ContactNo LIKE @SearchPattern OR 
                p.ProductName LIKE @SearchPattern OR 
                sdCash.IMENO LIKE @SearchPattern OR 
                sdCredit.IMENO LIKE @SearchPattern OR 
                sdCash.EngineNo LIKE @SearchPattern OR 
                sdCredit.EngineNo LIKE @SearchPattern OR 
                r.RegNo LIKE @SearchPattern OR 
                r.REGMRNo LIKE @SearchPattern
            )";
            parameters.Add("SearchPattern", $"%{request.Search.Trim()}%");
        }

        sql += " ORDER BY r.RegistrationID DESC";

        var list = await connection.QueryAsync<RegistrationListDto>(sql, parameters);
        return list.AsList();
    }
}

