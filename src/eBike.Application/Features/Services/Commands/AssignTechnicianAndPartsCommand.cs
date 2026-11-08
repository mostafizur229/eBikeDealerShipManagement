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

public record AssignTechnicianAndPartsResult(
    bool Success,
    int ServiceTicketId,
    int? SalesOrderId,
    string? SalesInvoiceNo,
    decimal PartsCost,
    decimal LaborCost,
    decimal TotalAmount,
    string Message
);

public record AssignTechnicianAndPartsCommand(
    int ServiceTicketId,
    int? AssignedTechnicianId,
    string? BayName,
    ServicePriority Priority,
    ServiceStatus? Status,
    int? CurrentMileageKm,
    string? IssueDescription,
    string? DiagnosisReport,
    string? WorkNotes,
    decimal EstimatedLaborCost,
    decimal Discount,
    decimal PaidAmount,
    DateTime? PromisedDeliveryDate,
    List<ServiceTicketItemInput>? UsedParts,
    string? Model = null,
    string? ChassisNumber = null
) : IRequest<AssignTechnicianAndPartsResult>;

public class AssignTechnicianAndPartsCommandHandler : IRequestHandler<AssignTechnicianAndPartsCommand, AssignTechnicianAndPartsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ISalesOrderExecutionService _salesOrderService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AssignTechnicianAndPartsCommandHandler> _logger;

    public AssignTechnicianAndPartsCommandHandler(
        IApplicationDbContext context,
        ISalesOrderExecutionService salesOrderService,
        ICurrentUser currentUser,
        ILogger<AssignTechnicianAndPartsCommandHandler> logger)
    {
        _context = context;
        _salesOrderService = salesOrderService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AssignTechnicianAndPartsResult> Handle(AssignTechnicianAndPartsCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.ServiceTickets
            .Include(t => t.UsedParts)
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == request.ServiceTicketId, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException(nameof(ServiceTicket), request.ServiceTicketId);
        }

        // 1. Update Technician, Status & Workshop SLA details
        if (request.AssignedTechnicianId.HasValue)
        {
            ticket.AssignedTechnicianId = request.AssignedTechnicianId.Value > 0 ? request.AssignedTechnicianId.Value : null;
        }

        if (request.Status.HasValue)
        {
            ticket.Status = request.Status.Value;
        }
        else if (ticket.Status == ServiceStatus.Pending)
        {
            ticket.Status = ServiceStatus.InProgress;
        }

        ticket.Priority = request.Priority;
        if (request.CurrentMileageKm.HasValue && request.CurrentMileageKm.Value > 0)
        {
            ticket.CurrentMileageKm = request.CurrentMileageKm.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.IssueDescription))
        {
            ticket.IssueDescription = request.IssueDescription.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.DiagnosisReport))
        {
            ticket.DiagnosisReport = request.DiagnosisReport.Trim();
        }

        ticket.LaborCost = request.EstimatedLaborCost;
        ticket.Discount = request.Discount;
        ticket.PaidAmount = request.PaidAmount;
        if (request.PromisedDeliveryDate.HasValue)
        {
            ticket.PromisedDeliveryDate = request.PromisedDeliveryDate.Value;
        }

        // Notes update with Bay, Model, Chassis
        var notesList = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.BayName))
            notesList.Add($"Bay: {request.BayName.Trim()}");
        if (!string.IsNullOrWhiteSpace(request.Model))
            notesList.Add($"Model: {request.Model.Trim()}");
        if (!string.IsNullOrWhiteSpace(request.ChassisNumber))
            notesList.Add($"Chassis: {request.ChassisNumber.Trim()}");

        var extraStr = notesList.Count > 0 ? "\n" + string.Join("\n", notesList) : "";
        if (!string.IsNullOrWhiteSpace(request.WorkNotes))
        {
            ticket.WorkNotes = $"{request.WorkNotes.Trim()}{extraStr}";
        }
        else if (!string.IsNullOrWhiteSpace(extraStr))
        {
            ticket.WorkNotes = $"{ticket.WorkNotes ?? ""}{extraStr}";
        }

        // 2. Used Parts Requisition & Immediate Stock Out Execution via SOType = 2 (Parts Sales Order)
        decimal partsTotal = 0;
        int targetConcernId = _currentUser.ConcernId ?? 3056;
        if (targetConcernId == 0) targetConcernId = 3056;
        int userId = _currentUser.UserId ?? 1;

        if (request.UsedParts != null && request.UsedParts.Count > 0)
        {
            ticket.UsedParts.Clear();

            var salesItems = new List<SalesOrderDetailExecutionItem>();

            foreach (var partItem in request.UsedParts)
            {
                var lineTotal = partItem.Quantity * partItem.UnitPrice;
                partsTotal += lineTotal;

                ticket.UsedParts.Add(new ServiceTicketItem
                {
                    ServiceTicketId = ticket.Id,
                    SparePartId = partItem.SparePartId,
                    Quantity = partItem.Quantity,
                    UnitPrice = partItem.UnitPrice
                });

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
                        Compressor = godownId,
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

            // Execute Parts Sales Order if not yet executed or if requisitioning parts
            if (salesItems.Count > 0 && !ticket.SalesOrderId.HasValue)
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
                        Status = 1, // Sales / Stock Out
                        CustomerId = ticket.CustomerId,
                        ConcernId = targetConcernId,
                        CreatedBy = userId,
                        CreateDate = DateTime.Now,
                        TotalOffer = 0,
                        NetDiscount = 0,
                        Remarks = $"Workshop Job Assignment & Parts Requisition: {ticket.TicketNumber} | Model: {request.Model ?? "Motorcycle"} | Chassis: {request.ChassisNumber ?? "N/A"}",
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
                        SOType = 2, // 2 = Spare Parts Sales Order (deducts stock from inventory)
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
                            _logger.LogInformation("Stock out executed! Created Parts Sales Order {InvoiceNo} (ID: {SOrderID}) for Service Ticket {TicketNumber}",
                                createdSOrder.InvoiceNo, createdSOrder.SOrderID, ticket.TicketNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing Parts Sales Order for Service Ticket {TicketNumber}", ticket.TicketNumber);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        decimal grandTotal = Math.Max(0, ticket.PartsCost + ticket.LaborCost - ticket.Discount);

        return new AssignTechnicianAndPartsResult(
            Success: true,
            ServiceTicketId: ticket.Id,
            SalesOrderId: ticket.SalesOrderId,
            SalesInvoiceNo: ticket.SalesInvoiceNo,
            PartsCost: ticket.PartsCost,
            LaborCost: ticket.LaborCost,
            TotalAmount: grandTotal,
            Message: ticket.SalesOrderId.HasValue
                ? $"Assignment saved and spare parts stocked out successfully (Invoice #{ticket.SalesInvoiceNo})."
                : "Assignment saved successfully."
        );
    }
}
