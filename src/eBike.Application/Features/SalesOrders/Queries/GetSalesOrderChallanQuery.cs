using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetSalesOrderChallanQuery(int OrderId) : IRequest<SalesOrderChallanDto>;

public class GetSalesOrderChallanQueryHandler : IRequestHandler<GetSalesOrderChallanQuery, SalesOrderChallanDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetSalesOrderChallanQueryHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<SalesOrderChallanDto> Handle(GetSalesOrderChallanQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.SOrders
            .AsNoTracking()
            .Include(s => s.Customer)
                .ThenInclude(c => c!.District)
            .Include(s => s.Customer)
                .ThenInclude(c => c!.ThanaObj)
            .Include(s => s.Customer)
                .ThenInclude(c => c!.Employee)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p!.Category)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p!.Company)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Color)
            .Include(s => s.SOrderDetails)
                .ThenInclude(d => d.StockDetail)
                    .ThenInclude(sd => sd!.Godown)
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

        var customer = order.Customer;
        decimal totalDue = customer?.TotalDue ?? order.TotalDue;
        decimal previousDue = totalDue - order.PaymentDue;

        var customerDto = new InvoiceCustomerDto
        {
            CustomerID = order.CustomerID,
            CustomerCode = customer?.Code ?? string.Empty,
            CustomerName = customer?.Name ?? string.Empty,
            ContactNo = customer?.ContactNo,
            Address = customer?.Address,
            PermanentAddress = customer?.PermanentAddress,
            FatherName = customer?.FName,
            MotherName = customer?.MName,
            DistrictName = customer?.District?.Name ?? customer?.Dist,
            ThanaName = customer?.ThanaObj?.Name ?? customer?.Thana,
            PostOffice = customer?.Post,
            RefName = customer?.RefName,
            RefFather = customer?.RefFName,
            RefContact = customer?.RefContact,
            RefAddress = customer?.RefAddress,
            PreviousDue = previousDue,
            TotalDue = totalDue,
            RemindDate = customer?.RemindDate
        };

        var partIds = order.SOrderDetails
            .Where(d => d.Product != null && d.Product.PartsNoID > 0)
            .Select(d => d.Product!.PartsNoID)
            .Distinct()
            .ToList();

        var locationIds = order.SOrderDetails
            .Where(d => d.Product != null && d.Product.PLocationID > 0)
            .Select(d => d.Product!.PLocationID)
            .Distinct()
            .ToList();

        var partsDict = partIds.Any()
            ? await _context.Parts.AsNoTracking()
                .Where(p => partIds.Contains(p.PartsNoID))
                .ToDictionaryAsync(p => p.PartsNoID, p => p.Description, cancellationToken)
            : new Dictionary<int, string?>();

        var locationDict = locationIds.Any()
            ? await _context.PLocations.AsNoTracking()
                .Where(l => locationIds.Contains(l.PLocationID))
                .ToDictionaryAsync(l => l.PLocationID, l => l.Description, cancellationToken)
            : new Dictionary<int, string?>();

        string soTypeName = order.SOType switch
        {
            2 => "Auto Parts",
            3 => "Credit",
            _ => "Vehicle"
        };

        var challanMetadata = new ChallanMetadataDto
        {
            SOrderID = order.SOrderID,
            ChallanNo = !string.IsNullOrWhiteSpace(order.BChallanNo) ? order.BChallanNo : order.InvoiceNo,
            ChallanDate = order.ChallanDate ?? order.InvoiceDate,
            InvoiceNo = order.InvoiceNo,
            InvoiceDate = order.InvoiceDate,
            SOType = order.SOType,
            SOTypeName = soTypeName,
            Remarks = order.Remarks,
            PrintDate = DateTime.Now
        };

        var salesPersonDto = new InvoiceSalesPersonDto
        {
            EmployeeID = customer?.EmployeeID,
            EmployeeName = customer?.Employee?.Name,
            EmployeeContact = customer?.Employee?.ContactNo,
            CreatedByID = order.CreatedBy,
            CreatedByName = createdByName
        };

        int sl = 1;
        var items = new List<SalesOrderChallanItemDto>();

        foreach (var detail in order.SOrderDetails)
        {
            var product = detail.Product;
            var stockDetail = detail.StockDetail;

            string? partNo = null;
            if (product != null && product.PartsNoID > 0)
            {
                partsDict.TryGetValue(product.PartsNoID, out partNo);
            }

            string? locationName = null;
            if (product != null && product.PLocationID > 0)
            {
                locationDict.TryGetValue(product.PLocationID, out locationName);
            }

            items.Add(new SalesOrderChallanItemDto
            {
                Sl = sl++,
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
                HorsePower = product?.HorsePower,
                SeatingCapacity = product?.SeatingCapacity,
                Weight = product?.Weight,

                PartNo = partNo,
                LocationName = locationName,

                Quantity = detail.Quantity,
                UnitType = product?.UnitType.HasValue == true ? product.UnitType.Value.ToString() : "Pcs",
                Remarks = detail.Remarks
            });
        }

        return new SalesOrderChallanDto
        {
            Company = sysInfo,
            Customer = customerDto,
            Challan = challanMetadata,
            SalesPerson = salesPersonDto,
            Items = items,
            TotalQuantity = items.Sum(i => i.Quantity)
        };
    }
}
