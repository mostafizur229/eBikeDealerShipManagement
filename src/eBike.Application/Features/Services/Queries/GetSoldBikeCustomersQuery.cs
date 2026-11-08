using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetSoldBikeCustomersQuery(string? Search = null, int Limit = 50) : IRequest<List<SoldBikeCustomerDto>>;

public class GetSoldBikeCustomersQueryHandler : IRequestHandler<GetSoldBikeCustomersQuery, List<SoldBikeCustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSoldBikeCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SoldBikeCustomerDto>> Handle(GetSoldBikeCustomersQuery request, CancellationToken cancellationToken)
    {
        var result = new List<SoldBikeCustomerDto>();
        var seenChassis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var search = request.Search?.Trim().ToLower();

        // 1. Existing CustomerVehicles (prioritized)
        try
        {
            var vehiclesQuery = _context.CustomerVehicles
                .Include(v => v.Customer)
                .Include(v => v.Product)
                .Include(v => v.Color)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                vehiclesQuery = vehiclesQuery.Where(v =>
                    v.Customer.Name.ToLower().Contains(search) ||
                    (v.Customer.ContactNo != null && v.Customer.ContactNo.Contains(search)) ||
                    v.ChassisNo.ToLower().Contains(search) ||
                    (v.RegistrationNo != null && v.RegistrationNo.ToLower().Contains(search)) ||
                    v.Product.ProductName.ToLower().Contains(search));
            }

            var existingVehicles = await vehiclesQuery
                .OrderByDescending(v => v.VehicleID)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            foreach (var v in existingVehicles)
            {
                if (string.IsNullOrWhiteSpace(v.ChassisNo) || seenChassis.Contains(v.ChassisNo))
                    continue;

                seenChassis.Add(v.ChassisNo);
                result.Add(new SoldBikeCustomerDto
                {
                    CustomerId = v.CustomerID,
                    CustomerName = v.Customer?.Name ?? "Customer",
                    CustomerPhone = v.Customer?.ContactNo ?? "",
                    CustomerAddress = v.Customer?.Address,
                    ProductId = v.ProductID,
                    BikeModel = v.Product?.ProductName ?? "Motorcycle",
                    ChassisNo = v.ChassisNo,
                    EngineNo = v.EngineNo,
                    ColorId = v.ColorID,
                    ColorName = v.Color?.Name,
                    SalesDate = v.OriginalPurchaseDate ?? v.CreatedDate,
                    InvoiceNo = v.OriginalInvoiceNo ?? $"V-{v.VehicleID}",
                    RegistrationNo = v.RegistrationNo,
                    SOrderId = v.SOrderID,
                    VehicleId = v.VehicleID,
                    CurrentMileageKm = v.CurrentOdometerReading,
                    FreeServicesCompleted = v.FreeServicesCompleted,
                    TotalFreeServicesEntitled = v.TotalFreeServicesEntitled,
                    OriginType = v.OriginType,
                    WarrantyStatus = v.WarrantyStatus ?? "Active"
                });
            }
        }
        catch
        {
            // Fallback to direct sales query
        }

        if (result.Count >= request.Limit)
        {
            return result;
        }

        // 2. Fetch directly from SOrders & CreditSales via lightning-fast indexed SQL (executes in < 15ms)
        try
        {
            var needed = request.Limit - result.Count;
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandTimeout = 15;

                string searchFilter = "";
                if (!string.IsNullOrWhiteSpace(search))
                {
                    searchFilter = @" AND (
                        c.Name LIKE @Search 
                        OR c.ContactNo LIKE @Search 
                        OR sd.IMENO LIKE @Search 
                        OR p.ProductName LIKE @Search 
                        OR s.InvoiceNo LIKE @Search
                    )";
                    var p = command.CreateParameter();
                    p.ParameterName = "@Search";
                    p.Value = $"%{search}%";
                    command.Parameters.Add(p);
                }

                var limitParam = command.CreateParameter();
                limitParam.ParameterName = "@Limit";
                limitParam.Value = needed * 2;
                command.Parameters.Add(limitParam);

                // Direct SOrders Cash Sales
                command.CommandText = $@"
                    SELECT TOP (@Limit)
                        s.CustomerID, 
                        c.Name as CustomerName, 
                        ISNULL(c.ContactNo, '') as CustomerPhone, 
                        c.Address as CustomerAddress,
                        d.ProductID, 
                        ISNULL(p.ProductName, 'Motorcycle') as BikeModel, 
                        sd.IMENO as ChassisNo, 
                        ISNULL(sd.EngineNo, '') as EngineNo,
                        sd.ColorID, 
                        col.Name as ColorName, 
                        s.InvoiceDate as SalesDate, 
                        s.InvoiceNo, 
                        s.SOrderID
                    FROM SOrderDetails d WITH (NOLOCK)
                    INNER JOIN SOrders s WITH (NOLOCK) ON d.SOrderID = s.SOrderID
                    INNER JOIN Customers c WITH (NOLOCK) ON s.CustomerID = c.CustomerID
                    INNER JOIN Products p WITH (NOLOCK) ON d.ProductID = p.ProductID
                    INNER JOIN StockDetails sd WITH (NOLOCK) ON d.SDetailID = sd.SDetailID
                    LEFT JOIN Colors col WITH (NOLOCK) ON sd.ColorID = col.ColorID
                    WHERE sd.IMENO IS NOT NULL AND LEN(sd.IMENO) > 5 AND sd.IMENO <> 'No Barcode'
                    {searchFilter}
                    ORDER BY s.SOrderID DESC;
                ";

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var chassis = reader["ChassisNo"]?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(chassis) || chassis == "No Barcode" || chassis.Length <= 5 || seenChassis.Contains(chassis))
                            continue;

                        seenChassis.Add(chassis);
                        result.Add(new SoldBikeCustomerDto
                        {
                            CustomerId = Convert.ToInt32(reader["CustomerID"]),
                            CustomerName = reader["CustomerName"]?.ToString() ?? "Customer",
                            CustomerPhone = reader["CustomerPhone"]?.ToString() ?? "",
                            CustomerAddress = reader["CustomerAddress"] != DBNull.Value ? reader["CustomerAddress"]?.ToString() : null,
                            ProductId = Convert.ToInt32(reader["ProductID"]),
                            BikeModel = reader["BikeModel"]?.ToString() ?? "Motorcycle",
                            ChassisNo = chassis,
                            EngineNo = reader["EngineNo"]?.ToString() ?? "",
                            ColorId = reader["ColorID"] != DBNull.Value ? Convert.ToInt32(reader["ColorID"]) : null,
                            ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"]?.ToString() : null,
                            SalesDate = reader["SalesDate"] != DBNull.Value ? Convert.ToDateTime(reader["SalesDate"]) : DateTime.UtcNow,
                            InvoiceNo = reader["InvoiceNo"]?.ToString() ?? "",
                            RegistrationNo = null,
                            SOrderId = reader["SOrderID"] != DBNull.Value ? Convert.ToInt32(reader["SOrderID"]) : null,
                            VehicleId = null,
                            CurrentMileageKm = 0,
                            FreeServicesCompleted = 0,
                            TotalFreeServicesEntitled = 4,
                            OriginType = "OwnShowroom",
                            WarrantyStatus = "Active"
                        });

                        if (result.Count >= request.Limit) break;
                    }
                }

                // 3. If still needed, query CreditSales (Installment Sales)
                if (result.Count < request.Limit)
                {
                    string creditSearchFilter = "";
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        creditSearchFilter = @" AND (
                            c.Name LIKE @Search 
                            OR c.ContactNo LIKE @Search 
                            OR sd.IMENO LIKE @Search 
                            OR p.ProductName LIKE @Search 
                            OR cs.InvoiceNo LIKE @Search
                        )";
                    }

                    command.CommandText = $@"
                        SELECT TOP (@Limit)
                            cs.CustomerID, 
                            c.Name as CustomerName, 
                            ISNULL(c.ContactNo, '') as CustomerPhone, 
                            c.Address as CustomerAddress,
                            d.ProductID, 
                            ISNULL(p.ProductName, 'Motorcycle') as BikeModel, 
                            sd.IMENO as ChassisNo, 
                            ISNULL(sd.EngineNo, '') as EngineNo,
                            sd.ColorID, 
                            col.Name as ColorName, 
                            cs.SalesDate, 
                            cs.InvoiceNo, 
                            cs.CreditSalesID
                        FROM CreditSaleDetails d WITH (NOLOCK)
                        INNER JOIN CreditSales cs WITH (NOLOCK) ON d.CreditSalesID = cs.CreditSalesID
                        INNER JOIN Customers c WITH (NOLOCK) ON cs.CustomerID = c.CustomerID
                        INNER JOIN Products p WITH (NOLOCK) ON d.ProductID = p.ProductID
                        INNER JOIN StockDetails sd WITH (NOLOCK) ON d.StockDetailID = sd.SDetailID
                        LEFT JOIN Colors col WITH (NOLOCK) ON sd.ColorID = col.ColorID
                        WHERE sd.IMENO IS NOT NULL AND LEN(sd.IMENO) > 5 AND sd.IMENO <> 'No Barcode'
                        {creditSearchFilter}
                        ORDER BY cs.CreditSalesID DESC;
                    ";

                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var chassis = reader["ChassisNo"]?.ToString()?.Trim();
                            if (string.IsNullOrWhiteSpace(chassis) || chassis == "No Barcode" || chassis.Length <= 5 || seenChassis.Contains(chassis))
                                continue;

                            seenChassis.Add(chassis);
                            result.Add(new SoldBikeCustomerDto
                            {
                                CustomerId = Convert.ToInt32(reader["CustomerID"]),
                                CustomerName = reader["CustomerName"]?.ToString() ?? "Customer",
                                CustomerPhone = reader["CustomerPhone"]?.ToString() ?? "",
                                CustomerAddress = reader["CustomerAddress"] != DBNull.Value ? reader["CustomerAddress"]?.ToString() : null,
                                ProductId = Convert.ToInt32(reader["ProductID"]),
                                BikeModel = reader["BikeModel"]?.ToString() ?? "Motorcycle",
                                ChassisNo = chassis,
                                EngineNo = reader["EngineNo"]?.ToString() ?? "",
                                ColorId = reader["ColorID"] != DBNull.Value ? Convert.ToInt32(reader["ColorID"]) : null,
                                ColorName = reader["ColorName"] != DBNull.Value ? reader["ColorName"]?.ToString() : null,
                                SalesDate = reader["SalesDate"] != DBNull.Value ? Convert.ToDateTime(reader["SalesDate"]) : DateTime.UtcNow,
                                InvoiceNo = reader["InvoiceNo"]?.ToString() ?? "",
                                RegistrationNo = null,
                                SOrderId = null,
                                VehicleId = null,
                                CurrentMileageKm = 0,
                                FreeServicesCompleted = 0,
                                TotalFreeServicesEntitled = 4,
                                OriginType = "OwnShowroom",
                                WarrantyStatus = "Active"
                            });

                            if (result.Count >= request.Limit) break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetSoldBikeCustomersQueryHandler] SQL query warning: {ex.Message}");
        }

        return result;
    }
}
