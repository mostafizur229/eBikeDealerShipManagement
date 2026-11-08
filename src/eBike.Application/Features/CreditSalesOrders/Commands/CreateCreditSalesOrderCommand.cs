using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditSalesOrders.DTOs;
using eBike.Application.Features.CreditSalesOrders.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditSalesOrders.Commands;

public class CreateCreditSalesOrderCommand : IRequest<CreditSalesOrderDto>
{
    public DateTime SalesDate { get; set; } = DateTime.Today;
    public DateTime InstallmentDate { get; set; } = DateTime.Today;
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DownPayment { get; set; }
    public decimal Remaining { get; set; }
    public int NoOfInstallment { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public string? Remarks { get; set; }
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public string? CBankName { get; set; }
    public string? CBBranch { get; set; }
    public string? CBChequeNo { get; set; }
    public string? CBNID { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int? BankId { get; set; }
    public int? CardTypeId { get; set; }
    public int? ConcernId { get; set; }

    public List<CreateCreditSalesOrderItemCommand> Details { get; set; } = new();
    public List<CreateCreditSalesOrderScheduleCommand> Schedules { get; set; } = new();
}

public class CreateCreditSalesOrderItemCommand
{
    public int ProductId { get; set; }
    public int StockDetailId { get; set; }
    public int? ColorId { get; set; }
    public int GodownId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal UTAmount { get; set; }
    public decimal MrpRate { get; set; }
    public decimal PPOffer { get; set; }
    public decimal IntPercentage { get; set; }
    public decimal IntTotalAmt { get; set; }
    public int RegType { get; set; }
    public decimal RegAmount { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public decimal InsuranceAmt { get; set; }
    public decimal OfferAmt { get; set; }
}

public class CreateCreditSalesOrderScheduleCommand
{
    public int ScheduleNo { get; set; }
    public DateTime MonthDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal NetValue { get; set; }
    public decimal HireValue { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public string PaymentStatus { get; set; } = "Due";
    public string? Remarks { get; set; }
}

public class CreateCreditSalesOrderCommandValidator : AbstractValidator<CreateCreditSalesOrderCommand>
{
    public CreateCreditSalesOrderCommandValidator()
    {
        RuleFor(x => x.InvoiceNo)
            .NotEmpty().WithMessage("Invoice No is required.")
            .MaximumLength(150);

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer is required.");

        RuleFor(x => x.NoOfInstallment)
            .GreaterThan(0).WithMessage("Number of installments must be greater than zero.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one vehicle item must be added to the credit sales order.");

        RuleFor(x => x.Details)
            .Must(details =>
            {
                var stockDetailIds = details.Select(d => d.StockDetailId).ToList();
                return stockDetailIds.Count == stockDetailIds.Distinct().Count();
            })
            .WithMessage("Duplicate vehicles/chassis detected in the order items.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ProductId)
                .GreaterThan(0).WithMessage("Product is required.");

            detail.RuleFor(d => d.StockDetailId)
                .GreaterThan(0).WithMessage("Vehicle Chassis selection is required.");

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

public class CreateCreditSalesOrderCommandHandler : IRequestHandler<CreateCreditSalesOrderCommand, CreditSalesOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICreditSalesOrderExecutionService _creditSalesOrderService;
    private readonly IMediator _mediator;

    public CreateCreditSalesOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICreditSalesOrderExecutionService creditSalesOrderService,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _creditSalesOrderService = creditSalesOrderService;
        _mediator = mediator;
    }

    public async Task<CreditSalesOrderDto> Handle(CreateCreditSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var concernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        var userId = _currentUser.UserId ?? 0;

        
        var customerExists = await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.CustomerID == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }


        var invoiceExists = await _creditSalesOrderService.IsInvoiceNoExistsAsync(
            request.InvoiceNo.Trim(), concernId, null, cancellationToken);

        if (invoiceExists)
        {
            request.InvoiceNo = await _creditSalesOrderService.GetNextInvoiceNoAsync(concernId, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.MRNo))
        {
            var mrExists = await _creditSalesOrderService.IsMRNoExistsAsync(request.MRNo.Trim(), concernId, null, cancellationToken);
            if (mrExists)
            {
                throw new ConflictException($"MR No '{request.MRNo}' already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.REGMRNo))
        {
            var regMrExists = await _creditSalesOrderService.IsREGMRNoExistsAsync(request.REGMRNo.Trim(), concernId, null, cancellationToken);
            if (regMrExists)
            {
                throw new ConflictException($"Registration MR No '{request.REGMRNo}' already exists.");
            }
        }

        foreach (var item in request.Details)
        {
            var isSold = await _creditSalesOrderService.IsChassisSoldAsync(item.StockDetailId, cancellationToken);
            if (isSold)
            {
                var vehicleInfo = await _context.StockDetails
                    .AsNoTracking()
                    .Where(sd => sd.SDetailID == item.StockDetailId)
                    .Select(sd => sd.IMENO)
                    .FirstOrDefaultAsync(cancellationToken);

                throw new ConflictException($"Vehicle Chassis '{vehicleInfo ?? item.StockDetailId.ToString()}' is already sold.");
            }
        }

        var schedulesToSave = new List<CreditSalesScheduleExecutionItem>();
        if (request.Schedules != null && request.Schedules.Count > 0)
        {
            foreach (var s in request.Schedules)
            {
                schedulesToSave.Add(new CreditSalesScheduleExecutionItem
                {
                    ScheduleNo = s.ScheduleNo,
                    MonthDate = s.MonthDate,
                    Balance = s.OpeningBalance,
                    InstallmentAmt = s.InstallmentAmount,
                    ClosingBalance = s.ClosingBalance,
                    PaymentStatus = string.IsNullOrWhiteSpace(s.PaymentStatus) ? "Due" : s.PaymentStatus,
                    Remarks = s.Remarks,
                    ExpectedInstallment = s.InstallmentAmount,
                    NetValue = s.NetValue,
                    HireValue = s.HireValue
                });
            }
        }
        else
        {
            var calcResult = await _mediator.Send(new CalculateInstallmentScheduleQuery(
                request.TotalAmount,
                request.DownPayment,
                request.NoOfInstallment,
                request.InstallmentDate,
                request.InterestAmount
            ), cancellationToken);

            foreach (var s in calcResult.Schedules)
            {
                schedulesToSave.Add(new CreditSalesScheduleExecutionItem
                {
                    ScheduleNo = s.ScheduleNo,
                    MonthDate = s.MonthDate,
                    Balance = s.OpeningBalance,
                    InstallmentAmt = s.InstallmentAmount,
                    ClosingBalance = s.ClosingBalance,
                    PaymentStatus = s.PaymentStatus,
                    ExpectedInstallment = s.InstallmentAmount,
                    NetValue = s.NetValue,
                    HireValue = s.HireValue
                });
            }
        }

        var cardTypeSetupId = 0;
        var depositChargePercent = 0m;
        BankTransExecutionItem? bankTrans = null;

        if (request.CardPaidAmount > 0 && request.BankId.HasValue && request.BankId.Value > 0)
        {
            var setup = await _creditSalesOrderService.GetCardSetupAsync(
                request.BankId.Value, request.CardTypeId ?? 0, cancellationToken);

            cardTypeSetupId = setup.CardTypeSetupId;
            depositChargePercent = setup.DepositChargePercent;

            var depositAmt = request.CardPaidAmount - ((request.CardPaidAmount * depositChargePercent) / 100m);

            bankTrans = new BankTransExecutionItem
            {
                TranDate = request.SalesDate,
                TransactionNo = request.InvoiceNo,
                TransactionType = 1,
                Amount = depositAmt,
                BankID = request.BankId.Value,
                CustomerID = 0,
                SupplierID = 0,
                AnotherBankID = 0,
                Remarks = "Card payment deposit transactions during credit sales.",
                ConcernID = concernId
            };
        }

        var executionModel = new CreditSalesOrderExecutionModel
        {
            InvoiceNo = request.InvoiceNo.Trim(),
            CustomerId = request.CustomerId,
            TSalesAmt = request.GrandTotal,
            NoOfInstallment = request.NoOfInstallment,
            InstallmentPrinciple = 0m,
            IssueDate = request.InstallmentDate,
            UserName = _currentUser.Username ?? string.Empty,
            Remaining = request.Remaining,
            InterestRate = request.InterestRate,
            InterestAmount = request.InterestAmount,
            SalesDate = request.SalesDate,
            DownPayment = request.DownPayment,
            WInterestAmt = 0m,
            FixedAmount = 0m,
            IsStatus = 1,
            UnExInstallment = 0,
            Quantity = (int)request.Details.Sum(d => d.Quantity),
            Discount = request.Discount,
            NetAmount = request.TotalAmount,
            IsUnExpected = false,
            Remarks = request.Remarks,
            Status = false,
            VatPercentage = request.VatPercentage,
            VatAmount = request.VatAmount,
            TotalDue = request.Remaining,
            ConcernId = concernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now,
            TotalOffer = request.Details.Sum(d => d.OfferAmt),
            InstallmentPeriod = $"{request.NoOfInstallment} Months",
            CardPaidAmount = request.CardPaidAmount,
            CardTypeSetupID = cardTypeSetupId,
            DepositChargePercent = depositChargePercent,
            MRNo = request.MRNo,
            CBankName = request.CBankName,
            CBBranch = request.CBBranch,
            CBChequeNo = request.CBChequeNo,
            LedgerNo = request.LedgerNo,
            CBNID = request.CBNID,
            REGMRNo = request.REGMRNo,
            BankID = request.BankId,
            CardTypeID = request.CardTypeId,
            BankTrans = bankTrans,
            Schedules = schedulesToSave,
            Details = request.Details.Select(d => new CreditSalesOrderDetailExecutionItem
            {
                ProductId = d.ProductId,
                StockDetailId = d.StockDetailId,
                ColorId = d.ColorId ?? 0,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                UTAmount = d.UTAmount,
                MpRateTotal = d.MrpRate,
                MrpRate = d.MrpRate,
                PPOffer = d.PPOffer,
                IntPercentage = d.IntPercentage,
                IntTotalAmt = d.IntTotalAmt,
                Compressor = d.GodownId,
                Motor = 0,
                Panel = 0,
                Spareparts = 0,
                Service = 0,
                Mushok = d.Mushok,
                SRateWMushok = d.SRateWMushok,
                RegAmt = d.RegAmount,
                OfferId = 0,
                OfferAmt = d.OfferAmt,
                InsuranceAmt = d.InsuranceAmt,
                RegType = d.RegType
            }).ToList()
        };

        var success = await _creditSalesOrderService.AddCreditSalesOrderAsync(executionModel, cancellationToken);
        if (!success)
        {
            throw new BadRequestException("Failed to save Credit Sales Order. Database stored procedure returned failure.");
        }

        var createdOrder = await _context.CreditSales
            .AsNoTracking()
            .Where(s => s.InvoiceNo == request.InvoiceNo.Trim() && s.ConcernID == concernId)
            .OrderByDescending(s => s.CreditSalesID)
            .Select(s => new { s.CreditSalesID })
            .FirstOrDefaultAsync(cancellationToken);

        if (createdOrder == null)
        {
            throw new BadRequestException("Credit sales order was saved, but could not retrieve generated record.");
        }

        if (request.Details.Any(d => d.RegAmount > 0))
        {
            var createdDetails = await _context.CreditSaleDetails
                .AsNoTracking()
                .Where(d => d.CreditSalesID == createdOrder.CreditSalesID)
                .Select(d => new { d.CreditSaleDetailsID, d.StockDetailID, d.ProductID })
                .ToListAsync(cancellationToken);

            foreach (var detail in request.Details.Where(d => d.RegAmount > 0))
            {
                var matched = createdDetails.FirstOrDefault(cd => cd.StockDetailID == detail.StockDetailId);
                var reg = new Registration
                {
                    ConcernID = concernId,
                    CustomerID = request.CustomerId,
                    SalesDate = request.SalesDate,
                    RegApplyDate = request.SalesDate,
                    ProductID = detail.ProductId,
                    RegFee = detail.RegAmount,
                    CreditSaleDetailsID = matched?.CreditSaleDetailsID,
                    Iscompleted = 1,
                    RegType = detail.RegType,
                    REGMRNo = request.REGMRNo,
                    WFStatus = 2,
                    CreateDate = DateTime.Now,
                    CreatedBy = userId
                };

                _context.Registrations.Add(reg);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return await _mediator.Send(new GetCreditSalesOrderByIdQuery(createdOrder.CreditSalesID), cancellationToken);
    }
}
