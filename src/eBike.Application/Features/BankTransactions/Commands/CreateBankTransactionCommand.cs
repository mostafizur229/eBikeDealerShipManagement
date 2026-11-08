using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.BankTransactions.Commands;

public record CreateBankTransactionCommand(
    DateTime TranDate,
    string? TransactionNo,
    int TransactionType,
    decimal Amount,
    int BankId,
    int? CustomerId = null,
    int? SupplierId = null,
    int? AnotherBankId = null,
    string? ChecqueNo = null,
    string? Remarks = null,
    int? ExpenseItemId = null,
    int? Sihid = null,
    int? ConcernId = null
) : IRequest<int>;

public class CreateBankTransactionCommandValidator : AbstractValidator<CreateBankTransactionCommand>
{
    public CreateBankTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transaction Amount must be greater than zero.");

        RuleFor(x => x.BankId)
            .GreaterThan(0).WithMessage("From A/C (Bank) is required.");

        RuleFor(x => x.TransactionType)
            .GreaterThan(0).WithMessage("Transaction Type is required.");

        RuleFor(x => x.TransactionNo)
            .MaximumLength(150).WithMessage("Transaction No must not exceed 150 characters.");

        RuleFor(x => x.ChecqueNo)
            .MaximumLength(150).WithMessage("Checque No must not exceed 150 characters.");

        RuleFor(x => x.Remarks)
            .MaximumLength(250).WithMessage("Remarks must not exceed 250 characters.");
    }
}

public class CreateBankTransactionCommandHandler : IRequestHandler<CreateBankTransactionCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;
    private readonly IBankBalanceService _bankBalanceService;

    public CreateBankTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender,
        IBankBalanceService bankBalanceService)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
        _bankBalanceService = bankBalanceService;
    }

    public async Task<int> Handle(CreateBankTransactionCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a bank transaction.");

        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.BankID == request.BankId, cancellationToken);

        if (bank == null)
        {
            throw new NotFoundException(nameof(Bank), request.BankId);
        }

        var tranType = (EnumTransactionType)request.TransactionType;

        if (tranType == EnumTransactionType.FundTransfer)
        {
            if (!request.AnotherBankId.HasValue || request.AnotherBankId.Value <= 0)
            {
                throw new BadRequestException("To A/C (Destination Bank) is required for Fund Transfer.");
            }

            if (request.AnotherBankId.Value == request.BankId)
            {
                throw new BadRequestException("From A/C and To A/C cannot be the same bank account.");
            }

            var destBank = await _context.Banks
                .FirstOrDefaultAsync(b => b.BankID == request.AnotherBankId.Value, cancellationToken);

            if (destBank == null)
            {
                throw new NotFoundException("Destination Bank", request.AnotherBankId.Value);
            }
        }
        else if (tranType == EnumTransactionType.CashCollection)
        {
            if (!request.CustomerId.HasValue || request.CustomerId.Value <= 0)
            {
                throw new BadRequestException("Customer is required for Cash Collection.");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId.Value, cancellationToken);

            if (customer == null)
            {
                throw new NotFoundException(nameof(Customer), request.CustomerId.Value);
            }
        }
        else if (tranType == EnumTransactionType.CashDelivery)
        {
            if (!request.SupplierId.HasValue || request.SupplierId.Value <= 0)
            {
                throw new BadRequestException("Supplier is required for Cash Delivery.");
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierId.Value, cancellationToken);

            if (supplier == null)
            {
                throw new NotFoundException(nameof(Supplier), request.SupplierId.Value);
            }
        }

        if (tranType == EnumTransactionType.Withdraw ||
            tranType == EnumTransactionType.CashDelivery ||
            tranType == EnumTransactionType.FundTransfer ||
            tranType == EnumTransactionType.BankExpense)
        {
            if (bank.IsAdvancedDueLimitApplicable == 1)
            {
                decimal limit = bank.AdvancedAmountLimit ?? 0m;
                if ((bank.TotalAmount - request.Amount) < -limit)
                {
                    throw new BadRequestException($"Transaction exceeds the allowed CC Loan limit of {limit:N2}.");
                }
            }
            else
            {
                if (bank.TotalAmount < request.Amount)
                {
                    throw new BadRequestException($"Insufficient bank balance. Available balance is {bank.TotalAmount:N2}.");
                }
            }
        }

        string transactionNo = request.TransactionNo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(transactionNo))
        {
            transactionNo = await _sender.Send(new Queries.GetNextBankTransactionNoQuery(concernId), cancellationToken);
        }

        int userId = _currentUser.UserId ?? 1;

        var transaction = new BankTransaction
        {
            TranDate = request.TranDate.Date,
            TransactionNo = transactionNo,
            TransactionType = request.TransactionType,
            Amount = request.Amount,
            BankID = request.BankId,
            CustomerID = request.CustomerId,
            SupplierID = request.SupplierId,
            AnotherBankID = request.AnotherBankId,
            ChecqueNo = request.ChecqueNo?.Trim(),
            Remarks = request.Remarks?.Trim(),
            ExpenseItemID = request.ExpenseItemId,
            SIHID = request.Sihid,
            ConcernID = concernId,
            WFStatus = (int)EnumWFStatus.Approved
        };

        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);

        return transaction.BankTranID;
    }
}
