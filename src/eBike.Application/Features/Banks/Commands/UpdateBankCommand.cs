using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Banks.Commands;

public record UpdateBankCommand(
    int BankID,
    string? Code,
    string BankName,
    string AccountNo,
    string BranchName,
    string AccountName,
    bool IsAdvancedDueLimitApplicable = false,
    decimal AdvancedAmountLimit = 0m
) : IRequest;

public class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.BankID)
            .GreaterThan(0).WithMessage("Bank ID is required.");

        RuleFor(x => x.BankName)
            .NotEmpty().WithMessage("Bank Name is required.")
            .MaximumLength(250).WithMessage("Bank Name must not exceed 250 characters.");

        RuleFor(x => x.AccountNo)
            .NotEmpty().WithMessage("Account No is required.")
            .MaximumLength(250).WithMessage("Account No must not exceed 250 characters.");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch Name is required.")
            .MaximumLength(250).WithMessage("Branch Name must not exceed 250 characters.");

        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account Name is required.")
            .MaximumLength(250).WithMessage("Account Name must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(150).WithMessage("Code must not exceed 150 characters.");
    }
}

public class UpdateBankCommandHandler : IRequestHandler<UpdateBankCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateBankCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await _context.Banks
            .FirstOrDefaultAsync(b => b.BankID == request.BankID, cancellationToken);

        if (bank == null)
        {
            throw new NotFoundException(nameof(Bank), request.BankID);
        }

        string bankName = request.BankName.Trim();
        string accountNo = request.AccountNo.Trim();

        var isDuplicate = await _context.Banks
            .AnyAsync(b => b.BankID != bank.BankID &&
                           b.ConcernID == bank.ConcernID &&
                           b.BankName.ToLower() == bankName.ToLower() &&
                           b.AccountNo.ToLower() == accountNo.ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Bank with the same name and account number already exists in this concern.");
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            var isDuplicateCode = await _context.Banks
                .AnyAsync(b => b.BankID != bank.BankID &&
                               b.ConcernID == bank.ConcernID &&
                               b.Code == code, cancellationToken);

            if (isDuplicateCode)
            {
                throw new ConflictException($"Bank Code '{code}' already exists in this concern.");
            }

            bank.Code = code;
        }

        bank.BankName = bankName;
        bank.AccountNo = accountNo;
        bank.BranchName = request.BranchName.Trim();
        bank.AccountName = request.AccountName.Trim();
        bank.IsAdvancedDueLimitApplicable = request.IsAdvancedDueLimitApplicable ? 1 : 0;
        bank.AdvancedAmountLimit = request.AdvancedAmountLimit;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
