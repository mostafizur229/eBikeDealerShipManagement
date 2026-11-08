using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Banks.Commands;

public record CreateBankCommand(
    string? Code,
    string BankName,
    string AccountNo,
    string BranchName,
    string AccountName,
    decimal OpeningBalance = 0m,
    bool IsAdvancedDueLimitApplicable = false,
    decimal AdvancedAmountLimit = 0m,
    int? ConcernId = null
) : IRequest<int>;

public class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
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

public class CreateBankCommandHandler : IRequestHandler<CreateBankCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateBankCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateBankCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a bank.");

        string bankName = request.BankName.Trim();
        string accountNo = request.AccountNo.Trim();

        var isDuplicate = await _context.Banks
            .AnyAsync(b => b.ConcernID == concernId &&
                           b.BankName.ToLower() == bankName.ToLower() &&
                           b.AccountNo.ToLower() == accountNo.ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Bank with the same name and account number already exists in this concern.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextBankCodeQuery(concernId), cancellationToken);
        }
        else
        {
            var isDuplicateCode = await _context.Banks
                .AnyAsync(b => b.ConcernID == concernId && b.Code == code, cancellationToken);

            if (isDuplicateCode)
            {
                code = await _sender.Send(new Queries.GetNextBankCodeQuery(concernId), cancellationToken);
            }
        }

        var bank = new Bank
        {
            Code = code,
            BankName = bankName,
            AccountNo = accountNo,
            BranchName = request.BranchName.Trim(),
            AccountName = request.AccountName.Trim(),
            OpeningBalance = request.OpeningBalance,
            TotalAmount = request.OpeningBalance,
            ConcernID = concernId,
            IsAdvancedDueLimitApplicable = request.IsAdvancedDueLimitApplicable ? 1 : 0,
            AdvancedAmountLimit = request.AdvancedAmountLimit
        };

        _context.Banks.Add(bank);
        await _context.SaveChangesAsync(cancellationToken);

        return bank.BankID;
    }
}
