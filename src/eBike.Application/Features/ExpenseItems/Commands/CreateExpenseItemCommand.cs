using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ExpenseItems.Commands;

public record CreateExpenseItemCommand(
    string? Code,
    string Description,
    int Status,
    int? ConcernId = null
) : IRequest<int>;

public class CreateExpenseItemCommandValidator : AbstractValidator<CreateExpenseItemCommand>
{
    public CreateExpenseItemCommandValidator()
    {
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

public class CreateExpenseItemCommandHandler : IRequestHandler<CreateExpenseItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateExpenseItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateExpenseItemCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create an Expense/Income Head.");

        string name = request.Description.Trim();

        var isDuplicate = await _context.ExpenseItems
            .AnyAsync(e => e.ConcernID == concernId && e.Description.ToLower() == name.ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException($"An Expense/Income Head with name '{name}' already exists in this concern.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextExpenseItemCodeQuery(concernId), cancellationToken);
        }
        else
        {
            var isDuplicateCode = await _context.ExpenseItems
                .AnyAsync(e => e.ConcernID == concernId && e.Code == code, cancellationToken);

            if (isDuplicateCode)
            {
                code = await _sender.Send(new Queries.GetNextExpenseItemCodeQuery(concernId), cancellationToken);
            }
        }

        int userId = _currentUser.UserId ?? 1;

        var item = new ExpenseItem
        {
            Code = code,
            Description = name,
            Status = request.Status,
            ConcernID = concernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now
        };

        _context.ExpenseItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return item.ExpenseItemID;
    }
}
