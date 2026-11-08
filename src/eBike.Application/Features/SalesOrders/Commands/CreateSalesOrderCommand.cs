using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesOrders.DTOs;
using eBike.Application.Features.SalesOrders.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesOrders.Commands;

public class CreateSalesOrderCommand : IRequest<SalesOrderDto>
{
    public DateTime InvoiceDate { get; set; } = DateTime.Today;
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
    public bool IsSmsEnable { get; set; }
    public int? ConcernId { get; set; }
    public int SOType { get; set; } = 1;

    public List<CreateSalesOrderItemCommand> Details { get; set; } = new();
}

public class CreateSalesOrderItemCommand
{
    public int ProductId { get; set; }
    public int StockDetailId { get; set; }
    public int? ColorId { get; set; }
    public int GodownId { get; set; }
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

public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.InvoiceNo)
            .NotEmpty().WithMessage("Invoice No is required.")
            .MaximumLength(150);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer is required.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one item must be added to the sales order.");

        When(x => x.SOType != 2, () =>
        {
            RuleFor(x => x.Details)
                .Must(details =>
                {
                    var stockDetailIds = details.Select(d => d.StockDetailId).ToList();
                    return stockDetailIds.Count == stockDetailIds.Distinct().Count();
                })
                .WithMessage("Duplicate vehicles/chassis detected in the order items.");
        });

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .GreaterThan(0).WithMessage("Product is required.");

            detail.RuleFor(d => d.StockDetailId)
                .GreaterThan(0).WithMessage("Stock item selection is required.");

            detail.RuleFor(d => d.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            detail.RuleFor(d => d.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Sales rate must be greater than or equal to zero.");
        });

        When(x => x.CardPaidAmount > 0, () =>
        {
            RuleFor(x => x.BankId)
                .NotNull().GreaterThan(0).WithMessage("Bank is required when Card payment is made.");
        });
    }
}

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, SalesOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISalesOrderExecutionService _salesOrderService;
    private readonly IMediator _mediator;

    public CreateSalesOrderCommandHandler(
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

    public async Task<SalesOrderDto> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        if (await _salesOrderService.IsInvoiceNoExistsAsync(request.InvoiceNo.Trim(), targetConcernId, null, cancellationToken))
        {
            request.InvoiceNo = await _salesOrderService.GetNextInvoiceNoAsync(targetConcernId, request.SOType, cancellationToken);
        }

        if (request.SOType == 2)
        {
            foreach (var item in request.Details)
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
        else
        {
            foreach (var item in request.Details)
            {
                if (await _salesOrderService.IsChassisSoldAsync(item.StockDetailId, cancellationToken))
                {
                    var stockDetail = await _context.StockDetails.AsNoTracking().FirstOrDefaultAsync(sd => sd.SDetailID == item.StockDetailId, cancellationToken);
                    var chassisNo = stockDetail?.IMENO ?? $"ID: {item.StockDetailId}";
                    throw new BadRequestException($"Vehicle with Chassis No '{chassisNo}' has already been sold or is not in stock.");
                }
            }
        }

        decimal totalOffer = request.Details.Sum(d => d.Quantity * d.PPOffer);
        decimal totalDisAmount = request.Details.Sum(d => d.PPDAmount);
        decimal netDiscount = totalDisAmount + totalOffer;

        decimal calculatedGrandTotal = request.Details.Sum(d => (d.UnitPrice + d.InterestAmt) * d.Quantity);
        decimal grandTotal = request.GrandTotal > 0 ? request.GrandTotal : calculatedGrandTotal;

        decimal netTotal = (grandTotal + request.VatAmount) - (netDiscount + request.AdjAmount);
        decimal paymentDue = netTotal - request.RecAmount;
        decimal totalDue = paymentDue;

        decimal totalMushok = request.Details.Sum(d => d.Mushok * d.Quantity);
        decimal totalInstAmt = request.Details.Sum(d => d.InterestAmt * d.Quantity);

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
            Status = 1, // Sales
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
            SOType = request.SOType,
            RemindDate = request.RemindDate,
            BankID = request.BankId,
            CardTypeID = request.CardTypeId,
            Details = request.Details.Select(d => new SalesOrderDetailExecutionItem
            {
                SOrderDetailID = 0,
                ProductId = d.ProductId,
                StockDetailId = d.StockDetailId,
                ColorId = d.ColorId,
                Status = 1,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                TAmount = d.UTAmount > 0 ? d.UTAmount : (d.UnitPrice * d.Quantity - d.PPDAmount),
                PPDisPer = d.PPDPercentage,
                PPDisAmt = d.PPDAmount,
                MrpRate = d.MrpRate,
                PPOffer = d.PPOffer,
                Compressor = d.GodownId, // mapped to GodownID
                Motor = d.RegType,       // mapped to RegType
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
                TransactionType = 1, // Deposit
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

        var success = await _salesOrderService.AddSalesOrderAsync(executionModel, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Failed to save Sales Order via database stored procedure.");
        }

        var createdOrder = await _context.SOrders
            .AsNoTracking()
            .Where(s => s.InvoiceNo == request.InvoiceNo.Trim() && s.ConcernID == targetConcernId)
            .OrderByDescending(s => s.SOrderID)
            .FirstOrDefaultAsync(cancellationToken);

        if (createdOrder == null)
        {
            throw new NotFoundException(nameof(SOrder), request.InvoiceNo);
        }

        if (request.Details.Any(d => d.RegAmount > 0))
        {
            var createdDetails = await _context.SOrderDetails
                .AsNoTracking()
                .Where(d => d.SOrderID == createdOrder.SOrderID)
                .Select(d => new { d.SOrderDetailID, d.SDetailID, d.ProductID })
                .ToListAsync(cancellationToken);

            foreach (var detail in request.Details.Where(d => d.RegAmount > 0))
            {
                var matched = createdDetails.FirstOrDefault(cd => cd.SDetailID == detail.StockDetailId);
                var reg = new Registration
                {
                    ConcernID = targetConcernId,
                    CustomerID = request.CustomerId,
                    SalesDate = request.InvoiceDate,
                    RegApplyDate = request.InvoiceDate,
                    ProductID = detail.ProductId,
                    RegFee = detail.RegAmount,
                    SorderDetailID = matched?.SOrderDetailID,
                    Iscompleted = 1, // Initiate
                    RegType = detail.RegType,
                    REGMRNo = request.REGMRNo,
                    WFStatus = 2, // Approved
                    CreateDate = DateTime.Now,
                    CreatedBy = _currentUser.UserId ?? 0
                };

                _context.Registrations.Add(reg);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return await _mediator.Send(new GetSalesOrderByIdQuery(createdOrder.SOrderID), cancellationToken);
    }
}
