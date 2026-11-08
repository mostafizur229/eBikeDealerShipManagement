using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.BankTransactions.Commands;

public record UpdateBankTransactionCommand(
    int BankTranID,
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
    int? Sihid = null
) : IRequest;

public class UpdateBankTransactionCommandValidator : AbstractValidator<UpdateBankTransactionCommand>
{
    public UpdateBankTransactionCommandValidator()
    {
        RuleFor(x => x.BankTranID)
            .GreaterThan(0).WithMessage("Transaction ID is required.");

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

public class UpdateBankTransactionCommandHandler : IRequestHandler<UpdateBankTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IBankBalanceService _bankBalanceService;

    public UpdateBankTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IBankBalanceService bankBalanceService)
    {
        _context = context;
        _currentUser = currentUser;
        _bankBalanceService = bankBalanceService;
    }

    public async Task Handle(UpdateBankTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.BankTransactions
            .FirstOrDefaultAsync(t => t.BankTranID == request.BankTranID, cancellationToken);

        if (transaction == null)
        {
            throw new NotFoundException(nameof(BankTransaction), request.BankTranID);
        }

        int userId = _currentUser.UserId ?? 1;

        await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: false, userId, cancellationToken);

        var bank = await _context.Banks
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BankID == request.BankId, cancellationToken);

        if (bank == null)
        {
            await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
            throw new NotFoundException(nameof(Bank), request.BankId);
        }

        var tranType = (EnumTransactionType)request.TransactionType;

        if (tranType == EnumTransactionType.FundTransfer)
        {
            if (!request.AnotherBankId.HasValue || request.AnotherBankId.Value <= 0)
            {
                await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
                throw new BadRequestException("To A/C (Destination Bank) is required for Fund Transfer.");
            }

            if (request.AnotherBankId.Value == request.BankId)
            {
                await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
                throw new BadRequestException("From A/C and To A/C cannot be the same bank account.");
            }
        }
        else if (tranType == EnumTransactionType.CashCollection)
        {
            if (!request.CustomerId.HasValue || request.CustomerId.Value <= 0)
            {
                await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
                throw new BadRequestException("Customer is required for Cash Collection.");
            }
        }
        else if (tranType == EnumTransactionType.CashDelivery)
        {
            if (!request.SupplierId.HasValue || request.SupplierId.Value <= 0)
            {
                await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
                throw new BadRequestException("Supplier is required for Cash Delivery.");
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
                    await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
                    throw new BadRequestException($"Transaction exceeds the allowed CC Loan limit of {limit:N2}.");
                }
            }
            else
            {
                if (bank.TotalAmount < request.Amount)
                {
                    await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
                    throw new BadRequestException($"Insufficient bank balance. Available balance is {bank.TotalAmount:N2}.");
                }
            }
        }

        transaction.TranDate = request.TranDate.Date;
        if (!string.IsNullOrWhiteSpace(request.TransactionNo))
        {
            transaction.TransactionNo = request.TransactionNo.Trim();
        }
        transaction.TransactionType = request.TransactionType;
        transaction.Amount = request.Amount;
        transaction.BankID = request.BankId;
        transaction.CustomerID = request.CustomerId;
        transaction.SupplierID = request.SupplierId;
        transaction.AnotherBankID = request.AnotherBankId;
        transaction.ChecqueNo = request.ChecqueNo?.Trim();
        transaction.Remarks = request.Remarks?.Trim();
        transaction.ExpenseItemID = request.ExpenseItemId;
        transaction.SIHID = request.Sihid;

        await _context.SaveChangesAsync(cancellationToken);

        await _bankBalanceService.UpdateBankBalanceAsync(transaction, isNewEntry: true, userId, cancellationToken);
    }
}
