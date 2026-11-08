using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Application.Features.SalesOrders.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Commands;

public class UpdateSalesOrderCommand : IRequest<SalesOrderDto>
{
    public int SOrderID { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDiscountPercentage { get; set; }
    public decimal TDiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal RecAmount { get; set; }
    public decimal AdjAmount { get; set; }
    public string? Remarks { get; set; }
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public DateTime? RemindDate { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int? BankId { get; set; }
    public int? CardTypeId { get; set; }
    public int? ConcernId { get; set; }
    public int? SOType { get; set; }

    public List<UpdateSalesOrderItemCommand> Details { get; set; } = new();
}

public class UpdateSalesOrderItemCommand
{
    public int SOrderDetailID { get; set; }
    public int ProductId { get; set; }
    public int StockDetailId { get; set; }
    public int? ColorId { get; set; }
    public int GodownId { get; set; }
    public int Status { get; set; } = 1; // 1 = Existing/Unchanged, 3 = Added/Modified, 5 = Deleted
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal PPDPercentage { get; set; }
    public decimal PPDAmount { get; set; }
    public decimal UTAmount { get; set; }
    public decimal MrpRate { get; set; }
    public decimal PPOffer { get; set; }
    public int RegType { get; set; }
    public decimal RegAmount { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmt { get; set; }
    public decimal InsuranceAmt { get; set; }
}

public class UpdateSalesOrderCommandValidator : AbstractValidator<UpdateSalesOrderCommand>
{
    public UpdateSalesOrderCommandValidator()
    {
        RuleFor(x => x.SOrderID)
            .GreaterThan(0).WithMessage("Invalid Sales Order ID.");

        RuleFor(x => x.InvoiceNo)
            .NotEmpty().WithMessage("Invoice No is required.")
            .MaximumLength(150);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer is required.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one item must be in the sales order.");
    }
}

public class UpdateSalesOrderCommandHandler : IRequestHandler<UpdateSalesOrderCommand, SalesOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISalesOrderExecutionService _salesOrderService;
    private readonly IMediator _mediator;

    public UpdateSalesOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISalesOrderExecutionService salesOrderService,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _salesOrderService = salesOrderService;
        _mediator = mediator;
    }

    public async Task<SalesOrderDto> Handle(UpdateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var existingOrder = await _context.SOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SOrderID == request.SOrderID, cancellationToken);

        if (existingOrder == null)
        {
            throw new NotFoundException(nameof(SOrder), request.SOrderID);
        }

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        if (await _salesOrderService.IsInvoiceNoExistsAsync(request.InvoiceNo.Trim(), targetConcernId, request.SOrderID, cancellationToken))
        {
            throw new ConflictException($"Invoice No '{request.InvoiceNo}' is already in use by another order.");
        }

        int resolvedSOType = request.SOType ?? existingOrder.SOType;

        if (resolvedSOType == 2)
        {
            foreach (var item in request.Details.Where(d => d.Status == 3))
            {
                var stockDetail = await _context.StockDetails.AsNoTracking()
                    .FirstOrDefaultAsync(sd => sd.SDetailID == item.StockDetailId, cancellationToken);

                if (stockDetail == null || stockDetail.Status != 1 || stockDetail.Quantity < item.Quantity)
                {
                    var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductID == item.ProductId, cancellationToken);
                    var productName = product?.ProductName ?? $"ID: {item.ProductId}";
                    var available = stockDetail?.Quantity ?? 0;
                    throw new BadRequestException($"Insufficient stock for part '{productName}'. Available: {available}, Requested: {item.Quantity}");
                }
            }
        }

        decimal totalOffer = request.Details.Where(d => d.Status != 5).Sum(d => d.Quantity * d.PPOffer);
        decimal totalDisAmount = request.Details.Where(d => d.Status != 5).Sum(d => d.PPDAmount);
        decimal netDiscount = totalDisAmount + totalOffer;

        decimal calculatedGrandTotal = request.Details.Where(d => d.Status != 5).Sum(d => (d.UnitPrice + d.InterestAmt) * d.Quantity);
        decimal grandTotal = request.GrandTotal > 0 ? request.GrandTotal : calculatedGrandTotal;

        decimal netTotal = (grandTotal + request.VatAmount) - (netDiscount + request.AdjAmount);
        decimal paymentDue = netTotal - request.RecAmount;
        decimal totalDue = paymentDue;

        decimal totalMushok = request.Details.Where(d => d.Status != 5).Sum(d => d.Mushok * d.Quantity);
        decimal totalInstAmt = request.Details.Where(d => d.Status != 5).Sum(d => d.InterestAmt * d.Quantity);

        var executionModel = new SalesOrderExecutionModel
        {
            InvoiceDate = request.InvoiceDate,
            InvoiceNo = request.InvoiceNo.Trim(),
            VatPercentage = request.VatPercentage,
            VatAmount = request.VatAmount,
            GrandTotal = grandTotal,
            TDiscountPercentage = request.TDiscountPercentage,
            TDiscountAmount = request.TDiscountAmount,
            RecAmt = request.RecAmount,
            PaymentDue = paymentDue,
            TotalAmount = netTotal,
            TotalDue = totalDue,
            AdjAmount = request.AdjAmount,
            Status = existingOrder.Status,
            CustomerId = request.CustomerId,
            ConcernId = targetConcernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now,
            TotalOffer = totalOffer,
            NetDiscount = netDiscount,
            Remarks = request.Remarks,
            CardPaidAmount = request.CardPaidAmount,
            CardTypeSetupID = request.CardTypeId ?? 0,
            DepositChargePercent = 0,
            IsOffer = totalOffer > 0 ? 1 : 0,
            TotalMushok = totalMushok,
            TotalInstAmt = totalInstAmt,
            TOfferAmt = 0,
            MRNo = request.MRNo,
            LedgerNo = request.LedgerNo,
            REGMRNo = request.REGMRNo,
            SOType = resolvedSOType,
            RemindDate = request.RemindDate,
            BankID = request.BankId,
            CardTypeID = request.CardTypeId,
            Details = request.Details.Select(d => new SalesOrderDetailExecutionItem
            {
                SOrderDetailID = d.SOrderDetailID,
                ProductId = d.ProductId,
                StockDetailId = d.StockDetailId,
                ColorId = d.ColorId,
                Status = d.Status,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TAmount = d.UTAmount > 0 ? d.UTAmount : (d.UnitPrice * d.Quantity - d.PPDAmount),
                PPDisPer = d.PPDPercentage,
                PPDisAmt = d.PPDAmount,
                MrpRate = d.MrpRate,
                PPOffer = d.PPOffer,
                Compressor = d.GodownId,
                Motor = d.RegType,
                Panel = 0,
                Spareparts = 0,
                Service = 0,
                RQuantity = 0,
                ReplaceProductType = 0,
                PPVat = 0,
                OfferAmt = 0,
                RegAmount = d.RegAmount,
                OfferID = 0,
                Mushok = d.Mushok,
                SRateWMushok = d.SRateWMushok,
                InterestRate = d.InterestRate,
                InterestAmt = d.InterestAmt,
                InsuranceAmt = d.InsuranceAmt
            }).ToList()
        };

        if (request.CardPaidAmount > 0 && request.BankId.HasValue)
        {
            executionModel.BankTrans = new BankTransExecutionItem
            {
                TranDate = request.InvoiceDate,
                TransactionNo = request.InvoiceNo.Trim(),
                TransactionType = 1,
                Amount = request.CardPaidAmount,
                BankID = request.BankId.Value,
                CustomerID = request.CustomerId,
                SupplierID = 0,
                AnotherBankID = 0,
                ChecqueNo = string.Empty,
                Remarks = "Card payment deposit transactions",
                ConcernID = targetConcernId
            };
        }

        var success = await _salesOrderService.UpdateSalesOrderAsync(request.SOrderID, userId, executionModel, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Failed to update Sales Order via database stored procedure.");
        }

        var currentDetails = await _context.SOrderDetails
            .AsNoTracking()
            .Where(d => d.SOrderID == request.SOrderID)
            .Select(d => new { d.SOrderDetailID, d.SDetailID, d.ProductID })
            .ToListAsync(cancellationToken);

        var existingRegs = await _context.Registrations
            .Where(r => r.SorderDetailID.HasValue && currentDetails.Select(cd => cd.SOrderDetailID).Contains(r.SorderDetailID.Value))
            .ToListAsync(cancellationToken);

        var activeStockDetailIdsWithReg = request.Details
            .Where(d => d.Status != 5 && d.RegAmount > 0)
            .Select(d => d.StockDetailId)
            .ToHashSet();

        var regsToRemove = existingRegs
            .Where(r => !currentDetails.Any(cd => cd.SOrderDetailID == r.SorderDetailID && activeStockDetailIdsWithReg.Contains(cd.SDetailID)))
            .ToList();

        if (regsToRemove.Count > 0)
        {
            _context.Registrations.RemoveRange(regsToRemove);
        }

        foreach (var item in request.Details.Where(d => d.Status != 5 && d.RegAmount > 0))
        {
            var matchedDetail = currentDetails.FirstOrDefault(cd => cd.SDetailID == item.StockDetailId);
            if (matchedDetail == null) continue;

            var existingReg = existingRegs.FirstOrDefault(r => r.SorderDetailID == matchedDetail.SOrderDetailID);
            if (existingReg != null)
            {
                existingReg.CustomerID = request.CustomerId;
                existingReg.SalesDate = request.InvoiceDate;
                existingReg.RegFee = item.RegAmount;
                existingReg.RegType = item.RegType;
                existingReg.REGMRNo = request.REGMRNo;
                existingReg.ModifiedDate = DateTime.Now;
                existingReg.ModifiedBy = userId;
            }
            else
            {
                var newReg = new Registration
                {
                    ConcernID = targetConcernId,
                    CustomerID = request.CustomerId,
                    SalesDate = request.InvoiceDate,
                    RegApplyDate = request.InvoiceDate,
                    ProductID = item.ProductId,
                    RegFee = item.RegAmount,
                    SorderDetailID = matchedDetail.SOrderDetailID,
                    Iscompleted = 1,
                    RegType = item.RegType,
                    REGMRNo = request.REGMRNo,
                    WFStatus = 2,
                    CreateDate = DateTime.Now,
                    CreatedBy = userId
                };
                _context.Registrations.Add(newReg);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetSalesOrderByIdQuery(request.SOrderID), cancellationToken);
    }
}
