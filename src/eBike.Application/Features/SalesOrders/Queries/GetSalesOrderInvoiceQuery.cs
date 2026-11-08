using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Helpers;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Queries;

public record GetSalesOrderInvoiceQuery(int OrderId, bool IsInvoice2 = false) : IRequest<SalesOrderInvoiceDto>;

public class GetSalesOrderInvoiceQueryHandler : IRequestHandler<GetSalesOrderInvoiceQuery, SalesOrderInvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public GetSalesOrderInvoiceQueryHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<SalesOrderInvoiceDto> Handle(GetSalesOrderInvoiceQuery request, CancellationToken cancellationToken)
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
                VATRegistrationNo,
                IsNewSInvoiceEnable
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

        string soTypeName = order.SOType switch
        {
            2 => "Auto Parts",
            3 => "Credit",
            _ => "Vehicle"
        };

        string statusName = Enum.IsDefined(typeof(EnumSalesType), order.Status)
            ? ((EnumSalesType)order.Status).ToString()
            : order.Status.ToString();

        string concernName = sysInfo.Name;
        int isNewSInvoiceEnable = sysInfo.IsNewSInvoiceEnable ?? 0;

        string primaryTemplateName;
        string primaryTemplatePath;

        if (order.SOType == 2) // Auto Parts
        {
            primaryTemplateName = "AMSalesInvoice";
            primaryTemplatePath = "AutoParts\\AMSalesInvoice.rdlc";
        }
        else // Vehicle
        {
            if (isNewSInvoiceEnable == 1)
            {
                primaryTemplateName = "AMSalesInvoice";
                primaryTemplatePath = "Sales\\AMSalesInvoice.rdlc";
            }
            else if (HasName(concernName, "NEW PARK"))
            {
                primaryTemplateName = "RetNewPInvoice";
                primaryTemplatePath = "NewParkInvoice\\RetNewPInvoice.rdlc";
            }
            else if (HasName(concernName, "T. R MOTORS", "T.R MOTORS", "TR MOTORS", "M.S AUTOMOBILE", "M.S. AUTOMOBILE", "MS AUTOMOBILE"))
            {
                primaryTemplateName = "AMSalesInvoiceTR";
                primaryTemplatePath = "Sales\\AMSalesInvoiceTR.rdlc";
            }
            else if (HasName(concernName, "MRIDHA"))
            {
                primaryTemplateName = "Mridha";
                primaryTemplatePath = "Sales\\Mridha.rdlc";
            }
            else
            {
                primaryTemplateName = "RetMaxInvoice";
                primaryTemplatePath = "Sales\\RetMaxInvoice.rdlc";
            }
        }

        bool hasInvoice2 = false;
        string? invoice2TemplateName = null;
        string? invoice2TemplatePath = null;

        if (order.SOType != 2) // Vehicle
        {
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
        }

        string activeTemplateName = (request.IsInvoice2 && hasInvoice2) ? invoice2TemplateName! : primaryTemplateName;
        string activeTemplatePath = (request.IsInvoice2 && hasInvoice2) ? invoice2TemplatePath! : primaryTemplatePath;

        var metadataDto = new InvoiceMetadataDto
        {
            SOrderID = order.SOrderID,
            InvoiceNo = order.InvoiceNo,
            InvoiceDate = order.InvoiceDate,
            SOType = order.SOType,
            SOTypeName = soTypeName,
            Status = order.Status,
            StatusName = statusName,
            MRNo = order.MRNo,
            LedgerNo = order.LedgerNo,
            REGMRNo = order.REGMRNo,
            Remarks = order.Remarks,
            PrintDate = DateTime.Now,
            TemplateName = activeTemplateName,
            TemplatePath = activeTemplatePath,
            HasInvoice2 = hasInvoice2,
            Invoice2TemplateName = invoice2TemplateName,
            Invoice2TemplatePath = invoice2TemplatePath
        };

        var salesPersonDto = new InvoiceSalesPersonDto
        {
            EmployeeID = customer?.EmployeeID,
            EmployeeName = customer?.Employee?.Name,
            EmployeeContact = customer?.Employee?.ContactNo,
            CreatedByID = order.CreatedBy,
            CreatedByName = createdByName
        };

        decimal recAmount = order.RecAmount ?? 0m;
        decimal cardAmount = order.CardPaidAmount;
        decimal cashAmount = Math.Max(0m, recAmount - cardAmount);
        decimal itemDiscountTotal = order.SOrderDetails.Sum(d => d.PPDAmount);
        decimal itemOfferTotal = order.SOrderDetails.Sum(d => d.Quantity * d.PPOffer);

        var financialsDto = new InvoiceFinancialsDto
        {
            GrandTotal = order.GrandTotal,
            ItemDiscountTotal = itemDiscountTotal,
            ItemOfferTotal = itemOfferTotal,
            VatPercentage = order.VATPercentage,
            VatAmount = order.VATAmount,
            TDPercentage = order.TDPercentage,
            TDAmount = order.TDAmount,
            LabourCost = 0m,
            AdjAmount = order.AdjAmount,
            NetDiscount = order.NetDiscount ?? (itemDiscountTotal + itemOfferTotal + order.TDAmount),
            TotalAmount = order.TotalAmount,
            RecAmount = recAmount,
            CardPaidAmount = cardAmount,
            CashPaidAmount = cashAmount,
            BankName = bankName,
            PaymentDue = order.PaymentDue,
            PreviousDue = previousDue,
            TotalDue = totalDue,
            InWordsTotal = NumberToWordsConverter.ToTakaWords(order.TotalAmount),
            InWordsPaid = NumberToWordsConverter.ToTakaWords(recAmount)
        };

        int sl = 1;
        var items = new List<SalesOrderInvoiceItemDto>();

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

            int regType = detail.RegType;
            string? regTypeName = Enum.IsDefined(typeof(EnumRegType), regType)
                ? ((EnumRegType)regType).ToString()
                : null;

            int godownId = stockDetail?.GodownID ?? (int.TryParse(detail.Compressor, out var gid) ? gid : 0);
            string? godownName = stockDetail?.Godown?.Name;

            items.Add(new SalesOrderInvoiceItemDto
            {
                Sl = sl++,
                SOrderDetailID = detail.SOrderDetailID,
                ProductID = detail.ProductID,
                ProductCode = product?.Code ?? string.Empty,
                ProductName = product?.ProductName ?? string.Empty,
                CategoryName = product?.Category?.Description,
                CompanyName = product?.Company?.Name,
                ColorName = stockDetail?.Color?.Name,
                GodownName = godownName,

                ChassisNo = stockDetail?.IMENO,
                EngineNo = stockDetail?.EngineNo,
                KeyNo = stockDetail?.KeyNo,
                CC = product?.CC,
                HorsePower = product?.HorsePower,
                SeatingCapacity = product?.SeatingCapacity,
                Weight = product?.Weight,
                VehicleSize = product?.VehicleSize,
                MakersName = product?.MakersName,
                MFGYear = product?.MFGYear,
                ApplicableModel = product?.ApplicableModel,
                RegType = regType,
                RegTypeName = regTypeName,
                RegAmount = detail.RegAmount,

                PartNo = partNo,
                LocationName = locationName,

                Quantity = detail.Quantity,
                UnitType = product?.UnitType.HasValue == true ? product.UnitType.Value.ToString() : "Pcs",
                UnitPrice = detail.UnitPrice,
                PPDPercentage = detail.PPDPercentage,
                PPDAmount = detail.PPDAmount,
                PPOffer = detail.PPOffer,
                TotalAmount = detail.UTAmount,

                CompressorWarranty = product?.CompressorWarrentyMonth,
                MotorWarranty = product?.MotorWarrentyMonth,
                ServiceWarranty = product?.ServiceWarrentyMonth,
                SparePartsWarranty = product?.SparePartsWarrentyMonth
            });
        }

        return new SalesOrderInvoiceDto
        {
            Company = sysInfo,
            Customer = customerDto,
            Order = metadataDto,
            SalesPerson = salesPersonDto,
            Financials = financialsDto,
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
