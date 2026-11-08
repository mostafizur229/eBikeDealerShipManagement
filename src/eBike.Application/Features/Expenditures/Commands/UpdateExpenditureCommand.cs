using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Expenditures.Commands;

public record UpdateExpenditureCommand(
    int ExpenditureID,
    DateTime EntryDate,
    string? VoucherNo,
    int ExpenseItemId,
    decimal Amount,
    string? Purpose = null,
    string? Remarks = null,
    int CustomerType = 0
) : IRequest;

public class UpdateExpenditureCommandValidator : AbstractValidator<UpdateExpenditureCommand>
{
    public UpdateExpenditureCommandValidator()
    {
        RuleFor(x => x.ExpenditureID)
            .GreaterThan(0).WithMessage("Expenditure ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.ExpenseItemId)
            .GreaterThan(0).WithMessage("Expense/Income Head is required.");

        RuleFor(x => x.VoucherNo)
            .MaximumLength(150).WithMessage("Voucher No must not exceed 150 characters.");

        RuleFor(x => x.Purpose)
            .MaximumLength(500).WithMessage("Purpose must not exceed 500 characters.");
    }
}

public class UpdateExpenditureCommandHandler : IRequestHandler<UpdateExpenditureCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateExpenditureCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateExpenditureCommand request, CancellationToken cancellationToken)
    {
        var expenditure = await _context.Expenditures
            .FirstOrDefaultAsync(e => e.ExpenditureID == request.ExpenditureID, cancellationToken);

        if (expenditure == null)
        {
            throw new NotFoundException(nameof(Expenditure), request.ExpenditureID);
        }

        var head = await _context.ExpenseItems
            .FirstOrDefaultAsync(h => h.ExpenseItemID == request.ExpenseItemId, cancellationToken);

        if (head == null)
        {
            throw new NotFoundException("Expense/Income Head", request.ExpenseItemId);
        }

        int userId = _currentUser.UserId ?? 1;

        expenditure.EntryDate = request.EntryDate.Date;
        if (!string.IsNullOrWhiteSpace(request.VoucherNo))
        {
            expenditure.VoucherNo = request.VoucherNo.Trim();
        }
        expenditure.ExpenseItemID = request.ExpenseItemId;
        expenditure.Amount = request.Amount;
        expenditure.Purpose = request.Purpose?.Trim();
        expenditure.Remarks = request.Remarks?.Trim();
        expenditure.CustomerType = request.CustomerType;
        expenditure.ModifiedBy = userId;
        expenditure.ModifiedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
