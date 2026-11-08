using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditSalesOrders.DTOs;
using eBike.Application.Features.CreditSalesOrders.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditSalesOrders.Commands;

public class PayInstallmentCommand : IRequest<CreditSalesOrderDto>
{
    public int CreditSalesId { get; set; }
    public int? CSScheduleId { get; set; }
    public bool IsAllPaid { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PayDate { get; set; } = DateTime.Today;
    public string? MRNo { get; set; }
    public decimal LastPayAdjustment { get; set; }
    public string? Remarks { get; set; }
    public DateTime? RemindDate { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int? BankId { get; set; }
    public int? CardTypeId { get; set; }
}

public class PayInstallmentCommandValidator : AbstractValidator<PayInstallmentCommand>
{
    public PayInstallmentCommandValidator()
    {
        RuleFor(x => x.CreditSalesId)
            .GreaterThan(0).WithMessage("Credit Sales Order ID is required.");

        RuleFor(x => x.PaidAmount)
            .GreaterThan(0).WithMessage("Paid amount must be greater than zero.");

        RuleFor(x => x.PayDate)
            .NotEmpty().WithMessage("Payment date is required.");

        When(x => x.CardPaidAmount > 0, () =>
        {
            RuleFor(x => x.BankId)
                .NotNull().GreaterThan(0).WithMessage("Bank is required for card payment.");
        });
    }
}

public class PayInstallmentCommandHandler : IRequestHandler<PayInstallmentCommand, CreditSalesOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICreditSalesOrderExecutionService _creditSalesOrderService;
    private readonly IMediator _mediator;

    public PayInstallmentCommandHandler(
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

    public async Task<CreditSalesOrderDto> Handle(PayInstallmentCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.CreditSales
            .AsNoTracking()
            .Include(s => s.CreditSalesSchedules)
            .FirstOrDefaultAsync(s => s.CreditSalesID == request.CreditSalesId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(CreditSale), request.CreditSalesId);
        }

        var allSchedules = order.CreditSalesSchedules.OrderBy(s => s.ScheduleNo).ToList();
        if (allSchedules.Count == 0)
        {
            throw new BadRequestException("No installment schedule found for this credit sales order.");
        }

        CreditSalesSchedule? targetSchedule = null;
        if (request.IsAllPaid)
        {
            targetSchedule = allSchedules.FirstOrDefault(s => s.PaymentStatus == "Due");
        }
        else if (request.CSScheduleId.HasValue && request.CSScheduleId.Value > 0)
        {
            targetSchedule = allSchedules.FirstOrDefault(s => s.CSScheduleID == request.CSScheduleId.Value && s.PaymentStatus == "Due");
        }
        else
        {
            targetSchedule = allSchedules.FirstOrDefault(s => s.PaymentStatus == "Due");
        }

        if (targetSchedule == null)
        {
            throw new BadRequestException("No due schedule available to pay.");
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
                TranDate = request.PayDate,
                TransactionNo = order.InvoiceNo,
                TransactionType = 1,
                Amount = depositAmt,
                BankID = request.BankId.Value,
                CustomerID = order.CustomerID,
                SupplierID = 0,
                AnotherBankID = 0,
                Remarks = "Card payment deposit transactions during installment payment.",
                ConcernID = order.ConcernID
            };
        }

        var updatedSchedules = new List<CreditSalesScheduleExecutionItem>();

        var paidAmount = request.PaidAmount;
        var lastPayAdj = request.LastPayAdjustment;

        targetSchedule.PaymentStatus = "Paid";
        targetSchedule.PaymentDate = request.PayDate;
        targetSchedule.MRNO = request.MRNo;
        targetSchedule.Remarks = request.Remarks;
        targetSchedule.InstallmentAmt = paidAmount;
        targetSchedule.LastPayAdjust = lastPayAdj;
        targetSchedule.CardPaidAmount = request.CardPaidAmount;
        targetSchedule.CardTypeSetupID = cardTypeSetupId;
        targetSchedule.DepositChargePercent = depositChargePercent;
        if (request.RemindDate.HasValue)
        {
            targetSchedule.RemindDate = request.RemindDate.Value;
        }

        var expectedAmount = targetSchedule.ExpectedInstallment > 0 ? targetSchedule.ExpectedInstallment : targetSchedule.InstallmentAmt;
        var totalDeduction = paidAmount + lastPayAdj;

        var newRemainingDue = Math.Max(0, targetSchedule.Balance - totalDeduction);
        targetSchedule.ClosingBalance = newRemainingDue;

        var remainingDueSchedules = allSchedules
            .Where(s => s.ScheduleNo > targetSchedule.ScheduleNo && s.PaymentStatus == "Due")
            .ToList();

        if (remainingDueSchedules.Count > 0 && newRemainingDue > 0)
        {
            var recomputedInstallment = Math.Round(newRemainingDue / remainingDueSchedules.Count, 2);
            var runningBalance = newRemainingDue;

            foreach (var dueSchedule in remainingDueSchedules)
            {
                dueSchedule.Balance = runningBalance;
                dueSchedule.InstallmentAmt = recomputedInstallment;
                var closing = Math.Max(0, runningBalance - recomputedInstallment);
                if (dueSchedule == remainingDueSchedules.Last())
                {
                    closing = 0m;
                }
                dueSchedule.ClosingBalance = closing;
                runningBalance = closing;
            }
        }
        else if (newRemainingDue <= 0)
        {
            foreach (var dueSchedule in remainingDueSchedules)
            {
                dueSchedule.Balance = 0m;
                dueSchedule.InstallmentAmt = 0m;
                dueSchedule.ClosingBalance = 0m;
                dueSchedule.PaymentStatus = "Paid";
                dueSchedule.PaymentDate = request.PayDate;
            }
        }

        foreach (var sc in allSchedules)
        {
            updatedSchedules.Add(new CreditSalesScheduleExecutionItem
            {
                CSScheduleID = sc.CSScheduleID,
                ScheduleNo = sc.ScheduleNo,
                MonthDate = sc.MonthDate,
                Balance = sc.Balance,
                InstallmentAmt = sc.InstallmentAmt,
                PaymentDate = sc.PaymentDate,
                PaymentStatus = sc.PaymentStatus,
                InterestAmount = sc.InterestAmount ?? 0m,
                ClosingBalance = sc.ClosingBalance,
                Remarks = sc.Remarks,
                IsUnExpected = sc.IsUnExpected,
                HireValue = sc.HireValue,
                NetValue = sc.NetValue,
                ExpectedInstallment = sc.ExpectedInstallment > 0 ? sc.ExpectedInstallment : sc.InstallmentAmt,
                CardPaidAmount = sc.CardPaidAmount,
                CardTypeSetupID = sc.CardTypeSetupID,
                DepositChargePercent = sc.DepositChargePercent,
                BankTransID = sc.BankTransID,
                MRNO = sc.MRNO,
                LastPayAdjustment = sc.LastPayAdjust
            });
        }

        var paymentModel = new CreditSalesInstallmentPaymentModel
        {
            SalesOrderId = request.CreditSalesId,
            InstallmentAmount = paidAmount,
            LastPayAdjustment = lastPayAdj,
            CardTypeSetupID = cardTypeSetupId,
            BankID = request.BankId,
            CardTypeID = request.CardTypeId,
            CardPaidAmount = request.CardPaidAmount,
            DepositChargePercent = depositChargePercent,
            Schedules = updatedSchedules,
            BankTrans = bankTrans
        };

        var success = await _creditSalesOrderService.InstallmentPaymentAsync(paymentModel, cancellationToken);
        if (!success)
        {
            throw new BadRequestException("Failed to process Installment Payment. Database procedure returned failure.");
        }

        return await _mediator.Send(new GetCreditSalesOrderByIdQuery(request.CreditSalesId), cancellationToken);
    }
}
