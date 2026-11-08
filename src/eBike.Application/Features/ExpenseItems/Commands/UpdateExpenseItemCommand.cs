using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ExpenseItems.Commands;

public record UpdateExpenseItemCommand(
    int ExpenseItemID,
    string? Code,
    string Description,
    int Status
) : IRequest;

public class UpdateExpenseItemCommandValidator : AbstractValidator<UpdateExpenseItemCommand>
{
    public UpdateExpenseItemCommandValidator()
    {
        RuleFor(x => x.ExpenseItemID)
            .GreaterThan(0).WithMessage("Expense Item ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Head Description/Name is required.")
            .MaximumLength(250).WithMessage("Description must not exceed 250 characters.");

        RuleFor(x => x.Status)
            .Must(s => s == (int)EnumCompanyTransaction.Expense || s == (int)EnumCompanyTransaction.Income)
            .WithMessage("Status must be either Expense (1) or Income (2).");

        RuleFor(x => x.Code)
            .MaximumLength(150).WithMessage("Code must not exceed 150 characters.");
    }
}

public class UpdateExpenseItemCommandHandler : IRequestHandler<UpdateExpenseItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateExpenseItemCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateExpenseItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ExpenseItems
            .FirstOrDefaultAsync(e => e.ExpenseItemID == request.ExpenseItemID, cancellationToken);

        if (item == null)
        {
            throw new NotFoundException(nameof(ExpenseItem), request.ExpenseItemID);
        }

        string name = request.Description.Trim();

        var isDuplicate = await _context.ExpenseItems
            .AnyAsync(e => e.ExpenseItemID != item.ExpenseItemID &&
                           e.ConcernID == item.ConcernID &&
                           e.Description.ToLower() == name.ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException($"An Expense/Income Head with name '{name}' already exists in this concern.");
        }
  
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            var isDuplicateCode = await _context.ExpenseItems
                .AnyAsync(e => e.ExpenseItemID != item.ExpenseItemID &&
                               e.ConcernID == item.ConcernID &&
                               e.Code == code, cancellationToken);

            if (isDuplicateCode)
            {
                throw new ConflictException($"Code '{code}' already exists in this concern.");
            }

            item.Code = code;
        }

        int userId = _currentUser.UserId ?? 1;

        item.Description = name;
        item.Status = request.Status;
        item.ModifiedBy = userId;
        item.ModifiedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
