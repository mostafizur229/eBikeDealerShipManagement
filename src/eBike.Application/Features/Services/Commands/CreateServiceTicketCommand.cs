using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eBike.Application.Features.Services.Commands;

public record ServiceTicketItemInput(
    int SparePartId,
    string? PartCode,
    string? PartName,
    int Quantity,
    decimal UnitPrice,
    int? StockDetailId = null,
    int? ProductId = null,
    int? GodownId = null,
    int? ColorId = null
);

public record CreateServiceTicketCommand(
    int? EBikeId,
    int? CustomerId,
    string? NewCustomerName,
    string? NewCustomerPhone,
    string? NewCustomerEmail,
    string? NewCustomerAddress,
    string? NewCustomerNid,
    string? BikeOriginType, // "OwnShowroom" or "ExternalShowroom"
    string? ExternalShowroomName,
    string? ExternalChassisNumber,
    string? ExternalModelName,
    int? AssignedTechnicianId,
    ServicePriority Priority,
    int CurrentMileageKm,
    string IssueDescription,
    string? DiagnosisReport,
    string? WorkNotes,
    decimal EstimatedLaborCost,
    decimal Discount,
    decimal PaidAmount,
    DateTime? PromisedDeliveryDate,
    List<ServiceTicketItemInput>? UsedParts,
    string? Model = null,
    string? ChassisNumber = null,
    string? BayName = null
) : IRequest<int>;

public class CreateServiceTicketCommandValidator : AbstractValidator<CreateServiceTicketCommand>
{
    public CreateServiceTicketCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.CustomerId.HasValue && x.CustomerId.Value > 0) || !string.IsNullOrWhiteSpace(x.NewCustomerName))
            .WithMessage("Either an existing customer must be selected or a new customer name must be provided.");

        RuleFor(x => x.IssueDescription)
            .NotEmpty()
            .WithMessage("Issue description / service complaint is required.");

        RuleFor(x => x.CurrentMileageKm)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Current mileage cannot be negative.");
    }
}

public class CreateServiceTicketCommandHandler : IRequestHandler<CreateServiceTicketCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISalesOrderExecutionService _salesOrderService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CreateServiceTicketCommandHandler> _logger;

    public CreateServiceTicketCommandHandler(
        IApplicationDbContext context,
        ISalesOrderExecutionService salesOrderService,
        ICurrentUser currentUser,
        ILogger<CreateServiceTicketCommandHandler> logger)
    {
        _context = context;
        _salesOrderService = salesOrderService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<int> Handle(CreateServiceTicketCommand request, CancellationToken cancellationToken)
    {
        int customerId;

        // 1. Resolve Customer: Existing vs New Customer
        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerID == request.CustomerId.Value, cancellationToken);
            if (!customerExists)
            {
                throw new NotFoundException(nameof(Customer), request.CustomerId.Value);
            }
            customerId = request.CustomerId.Value;
        }
        else
        {
            var newCustomer = new Customer
            {
                Code = $"CUS-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = request.NewCustomerName!.Trim(),
                ContactNo = request.NewCustomerPhone?.Trim() ?? "",
                EmailID = request.NewCustomerEmail?.Trim(),
                Address = request.NewCustomerAddress?.Trim(),
                NID = request.NewCustomerNid?.Trim(),
                DistrictID = 1,
                ThanaID = 1,
                EmployeeID = 1,
                ConcernID = 1
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync(cancellationToken);
            customerId = newCustomer.CustomerID;
        }

        // 2. EBike status update if registered
        if (request.EBikeId.HasValue && request.EBikeId.Value > 0)
        {
            var eBike = await _context.EBikes.FirstOrDefaultAsync(b => b.Id == request.EBikeId.Value, cancellationToken);
            if (eBike != null)
            {
                eBike.Status = EBikeStatus.InService;
                eBike.CurrentOdometerKm = request.CurrentMileageKm;
            }
        }

        // 3. Generate Ticket Number: SRV-YYYYMM-XXXX
        var prefix = $"SRV-{DateTime.UtcNow:yyyyMM}-";
        var countThisMonth = await _context.ServiceTickets
            .CountAsync(s => s.TicketNumber.StartsWith(prefix), cancellationToken);

        var ticketNumber = $"{prefix}{(countThisMonth + 1):D4}";

        // 4. Notes regarding Bike Origin
        var originNote = request.BikeOriginType == "ExternalShowroom"
            ? $"[Origin: External Showroom - {request.ExternalShowroomName ?? "Other Dealership"}] Chassis: {request.ExternalChassisNumber ?? "N/A"}"
            : "[Origin: Own Showroom]";

        string? resolvedModel = !string.IsNullOrWhiteSpace(request.Model) 
            ? request.Model 
            : request.ExternalModelName;
        string? resolvedChassis = !string.IsNullOrWhiteSpace(request.ChassisNumber) 
            ? request.ChassisNumber 
            : request.ExternalChassisNumber;

        var extraNotes = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.BayName))
            extraNotes.Add($"Bay: {request.BayName.Trim()}");
        if (!string.IsNullOrWhiteSpace(resolvedModel))
            extraNotes.Add($"Model: {resolvedModel.Trim()}");
        if (!string.IsNullOrWhiteSpace(resolvedChassis))
            extraNotes.Add($"Chassis: {resolvedChassis.Trim()}");

        var extraStr = extraNotes.Count > 0 ? "\n" + string.Join("\n", extraNotes) : "";

        var combinedNotes = string.IsNullOrWhiteSpace(request.WorkNotes)
            ? $"{originNote}{extraStr}"
            : $"{originNote}{extraStr}\n{request.WorkNotes}";

        var ticket = new ServiceTicket
        {
            TicketNumber = ticketNumber,
            EBikeId = request.EBikeId,
            CustomerId = customerId,
            AssignedTechnicianId = (request.AssignedTechnicianId.HasValue && request.AssignedTechnicianId.Value > 0) ? request.AssignedTechnicianId.Value : null,
            Status = ServiceStatus.Pending,
            Priority = request.Priority,
            CurrentMileageKm = request.CurrentMileageKm,
            IssueDescription = request.IssueDescription,
            DiagnosisReport = request.DiagnosisReport,
            WorkNotes = combinedNotes,
            LaborCost = request.EstimatedLaborCost,
            PartsCost = 0,
            Discount = request.Discount,
            PaidAmount = request.PaidAmount,
            PromisedDeliveryDate = request.PromisedDeliveryDate
        };

        // 5. Add Used Parts / Products with Selling Prices & Deduct from Stock via Parts Sales Order (SOType = 2)
        decimal partsTotal = 0;
        int targetConcernId = _currentUser.ConcernId ?? 3056;
        if (targetConcernId == 0) targetConcernId = 3056;
        int userId = _currentUser.UserId ?? 1;

        if (request.UsedParts != null && request.UsedParts.Count > 0)
        {
            var salesItems = new List<SalesOrderDetailExecutionItem>();

            foreach (var partItem in request.UsedParts)
            {
                var lineTotal = partItem.Quantity * partItem.UnitPrice;
                partsTotal += lineTotal;

                ticket.UsedParts.Add(new ServiceTicketItem
                {
                    ServiceTicket = ticket,
                    SparePartId = partItem.SparePartId,
                    Quantity = partItem.Quantity,
                    UnitPrice = partItem.UnitPrice
                });

                // Find or verify stock detail
                int stockDetailId = partItem.StockDetailId ?? 0;
                int godownId = partItem.GodownId ?? 1;
                int? colorId = partItem.ColorId;
                int productId = partItem.ProductId ?? (partItem.SparePartId > 0 ? partItem.SparePartId : 0);

                if (stockDetailId <= 0 && productId > 0)
                {
                    var foundStock = await _context.StockDetails.AsNoTracking()
                        .Where(sd => sd.ProductID == productId && sd.Status == 1 && sd.Quantity >= partItem.Quantity)
                        .OrderByDescending(sd => sd.Quantity)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (foundStock != null)
                    {
                        stockDetailId = foundStock.SDetailID;
                        godownId = foundStock.GodownID;
                        colorId = foundStock.ColorID;
                    }
                }

                if (stockDetailId > 0 && productId > 0)
                {
                    salesItems.Add(new SalesOrderDetailExecutionItem
                    {
                        SOrderDetailID = 0,
                        ProductId = productId,
                        StockDetailId = stockDetailId,
                        ColorId = colorId,
                        Status = 1,
                        Quantity = partItem.Quantity,
                        UnitPrice = partItem.UnitPrice,
                        TAmount = lineTotal,
                        PPDisPer = 0,
                        PPDisAmt = 0,
                        MrpRate = partItem.UnitPrice,
                        PPOffer = 0,
                        Compressor = godownId, // mapped to GodownID
                        Motor = 0,
                        Panel = 0,
                        Spareparts = 0,
                        Service = 0,
                        RQuantity = 0,
                        ReplaceProductType = 0,
                        PPVat = 0,
                        OfferAmt = 0,
                        RegAmount = 0,
                        OfferID = 0,
                        Mushok = 0,
                        SRateWMushok = partItem.UnitPrice,
                        InterestRate = 0,
                        InterestAmt = 0,
                        InsuranceAmt = 0
                    });
                }
            }

            ticket.PartsCost = partsTotal;

            if (salesItems.Count > 0)
            {
                try
                {
                    var nextPartsInvoiceNo = await _salesOrderService.GetNextInvoiceNoAsync(targetConcernId, 2, cancellationToken);
                    decimal recAmt = Math.Min(request.PaidAmount, partsTotal);
                    decimal paymentDue = partsTotal - recAmt;

                    var executionModel = new SalesOrderExecutionModel
                    {
                        InvoiceDate = DateTime.Today,
                        InvoiceNo = nextPartsInvoiceNo.Trim(),
                        VatPercentage = 0,
                        VatAmount = 0,
                        GrandTotal = partsTotal,
                        TDiscountPercentage = 0,
                        TDiscountAmount = 0,
                        RecAmt = recAmt,
                        PaymentDue = paymentDue,
                        TotalAmount = partsTotal,
                        TotalDue = paymentDue,
                        AdjAmount = 0,
                        Status = 1, // Sales
                        CustomerId = customerId,
                        ConcernId = targetConcernId,
                        CreatedBy = userId,
                        CreateDate = DateTime.Now,
                        TotalOffer = 0,
                        NetDiscount = 0,
                        Remarks = $"Workshop Job Card Service Ticket: {ticketNumber} | Bike: {request.ExternalModelName ?? "Motorcycle"} | Chassis: {request.ExternalChassisNumber ?? "N/A"}",
                        CardPaidAmount = 0,
                        CardTypeSetupID = 0,
                        DepositChargePercent = 0,
                        IsOffer = 0,
                        TotalMushok = 0,
                        TotalInstAmt = 0,
                        TOfferAmt = 0,
                        MRNo = null,
                        LedgerNo = null,
                        REGMRNo = null,
                        SOType = 2, // 2 = Spare Parts Sales Order
                        RemindDate = null,
                        BankID = null,
                        CardTypeID = null,
                        Details = salesItems
                    };

                    var soSuccess = await _salesOrderService.AddSalesOrderAsync(executionModel, cancellationToken);
                    if (soSuccess)
                    {
                        var createdSOrder = await _context.SOrders
                            .AsNoTracking()
                            .Where(s => s.InvoiceNo == nextPartsInvoiceNo.Trim() && s.ConcernID == targetConcernId)
                            .OrderByDescending(s => s.SOrderID)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (createdSOrder != null)
                        {
                            ticket.SalesOrderId = createdSOrder.SOrderID;
                            ticket.SalesInvoiceNo = createdSOrder.InvoiceNo;
                            _logger.LogInformation("Created Parts Sales Order {InvoiceNo} (ID: {SOrderID}) and deducted inventory for Service Ticket {TicketNumber}",
                                createdSOrder.InvoiceNo, createdSOrder.SOrderID, ticketNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing Parts Sales Order for Service Ticket {TicketNumber}", ticketNumber);
                }
            }
        }

        _context.ServiceTickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        return ticket.Id;
    }
}
