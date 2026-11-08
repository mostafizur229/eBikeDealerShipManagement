using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Helpers;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseOrders.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrderInvoiceQuery(int OrderId) : IRequest<PurchaseOrderInvoiceDto>;

public class GetPurchaseOrderInvoiceQueryHandler : IRequestHandler<GetPurchaseOrderInvoiceQuery, PurchaseOrderInvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetPurchaseOrderInvoiceQueryHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PurchaseOrderInvoiceDto> Handle(GetPurchaseOrderInvoiceQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.POrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(pr => pr!.Category)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(pr => pr!.Company)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Color)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.Godown)
            .Include(p => p.POrderDetails)
                .ThenInclude(d => d.POProductDetails)
            .FirstOrDefaultAsync(p => p.POrderID == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(POrder), request.OrderId);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        var sysInfo = await connection.QueryFirstOrDefaultAsync<PurchaseOrderCompanyDto>(@"
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

            sysInfo = new PurchaseOrderCompanyDto
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

        var partIds = order.POrderDetails
            .Where(d => d.Product != null && d.Product.PartsNoID > 0)
            .Select(d => d.Product!.PartsNoID)
            .Distinct()
            .ToList();

        var locationIds = order.POrderDetails
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

        var supplier = order.Supplier;
        var supplierDto = new PurchaseOrderSupplierDto
        {
            SupplierID = order.SupplierID,
            SupplierCode = supplier?.Code ?? string.Empty,
            SupplierName = supplier?.Name ?? string.Empty,
            Address = supplier?.Address,
            ContactNo = supplier?.ContactNo,
            TotalDue = supplier?.TotalDue ?? order.TotalDue
        };

        string soTypeName = order.POType switch
        {
            2 => "Auto Parts",
            _ => "Vehicle"
        };

        string statusName = Enum.IsDefined(typeof(EnumPurchaseType), order.Status)
            ? ((EnumPurchaseType)order.Status).ToString()
            : order.Status.ToString();

        string reportHeader;
        if (order.Status == (int)EnumPurchaseType.DeliveryOrder)
        {
            reportHeader = "Delivery Order";
        }
        else if (order.Status == (int)EnumPurchaseType.DamageReturn)
        {
            reportHeader = "Damage Return Order";
        }
        else if (order.Status == (int)EnumPurchaseType.Purchase && order.IsDamageOrder == 1)
        {
            reportHeader = "Damage Order";
        }
        else if (order.Status == (int)EnumPurchaseType.ProductReturn)
        {
            reportHeader = "Purchase Return Order";
        }
        else
        {
            reportHeader = "Purchase Order";
        }

        var metadataDto = new PurchaseOrderMetadataDto
        {
            POrderID = order.POrderID,
            ChallanNo = order.ChallanNo,
            OrderDate = order.OrderDate,
            POType = order.POType,
            POTypeName = soTypeName,
            ReportHeader = reportHeader,
            Status = order.Status,
            StatusName = statusName,
            IsDamageOrder = order.IsDamageOrder,
            Remarks = order.Remarks,
            PrintDate = DateTime.Now,
            CreatedByID = order.CreatedBy,
            CreatedByName = createdByName
        };

        var financialsDto = new PurchaseOrderFinancialsDto
        {
            GrandTotal = order.GrandTotal,
            TDiscount = order.TDiscount,
            PPDisAmt = order.PPDisAmt,
            NetDiscount = order.NetDiscount,
            LaborCost = order.LaborCost,
            TotalMushok = order.TotalMushok,
            TotalTax = order.TotalTax,
            TotalAmt = order.TotalAmt,
            RecAmt = order.RecAmt,
            PaymentDue = order.PaymentDue,
            TotalDue = supplier?.TotalDue ?? order.TotalDue,
            InWordsTotal = NumberToWordsConverter.ToTakaWords(order.TotalAmt),
            InWordsPaid = NumberToWordsConverter.ToTakaWords(order.RecAmt)
        };

        int sl = 1;
        var items = new List<PurchaseOrderInvoiceItemDto>();

        foreach (var detail in order.POrderDetails)
        {
            var product = detail.Product;

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

            var chassisList = detail.POProductDetails
                .Where(s => !string.IsNullOrWhiteSpace(s.IMENO))
                .Select(s => s.IMENO.Trim())
                .ToList();

            var engineList = detail.POProductDetails
                .Where(s => !string.IsNullOrWhiteSpace(s.EngineNo))
                .Select(s => s.EngineNo.Trim())
                .ToList();

            items.Add(new PurchaseOrderInvoiceItemDto
            {
                Sl = sl++,
                POrderDetailID = detail.POrderDetailID,
                ProductID = detail.ProductID,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.ProductName ?? string.Empty,
                CategoryName = product?.Category?.Description,
                CompanyName = product?.Company?.Name,
                ColorName = detail.Color?.Name,
                GodownName = detail.Godown?.Name,

                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                DisPer = detail.PPDISPer,
                DisAmt = detail.PPDISAmt,
                ExtraDisPer = detail.ExtraPPDISPer,
                ExtraDisAmt = detail.ExtraPPDISAmt,
                TotalAmt = detail.TAmount,


                DealerUnitPrice = detail.UnitPrice,
                HolTax = detail.PWTaxAmt,
                ChassisNumbers = chassisList,
                EngineNumbers = engineList,
                ChassisNumbersText = string.Join(Environment.NewLine, chassisList),
                EngineNumbersText = string.Join(Environment.NewLine, engineList),

                PartNo = partNo,
                LocationName = locationName
            });
        }

        return new PurchaseOrderInvoiceDto
        {
            Company = sysInfo,
            Supplier = supplierDto,
            Order = metadataDto,
            Financials = financialsDto,
            Items = items
        };
    }
}
