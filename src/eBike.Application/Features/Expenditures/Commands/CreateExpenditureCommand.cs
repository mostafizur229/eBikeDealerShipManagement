using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Expenditures.Commands;

public record CreateExpenditureCommand(
    DateTime EntryDate,
    string? VoucherNo,
    int ExpenseItemId,
    decimal Amount,
    string? Purpose = null,
    string? Remarks = null,
    int CustomerType = 0,
    int? ConcernId = null
) : IRequest<int>;

public class CreateExpenditureCommandValidator : AbstractValidator<CreateExpenditureCommand>
{
    public CreateExpenditureCommandValidator()
    {
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

public class CreateExpenditureCommandHandler : IRequestHandler<CreateExpenditureCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateExpenditureCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateExpenditureCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create an Expenditure/Income voucher.");

        var head = await _context.ExpenseItems
            .FirstOrDefaultAsync(h => h.ExpenseItemID == request.ExpenseItemId, cancellationToken);

        if (head == null)
        {
            throw new NotFoundException("Expense/Income Head", request.ExpenseItemId);
        }

        string voucherNo = request.VoucherNo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(voucherNo))
        {
            voucherNo = await _sender.Send(new Queries.GetNextVoucherNoQuery(concernId), cancellationToken);
        }

        int userId = _currentUser.UserId ?? 1;

        var expenditure = new Expenditure
        {
            EntryDate = request.EntryDate.Date,
            VoucherNo = voucherNo,
            ExpenseItemID = request.ExpenseItemId,
            Amount = request.Amount,
            Purpose = request.Purpose?.Trim(),
            Remarks = request.Remarks?.Trim(),
            CustomerType = request.CustomerType,
            ConcernID = concernId,
            WFStatus = (int)EnumWFStatus.Approved,
            CreatedBy = userId,
            CreateDate = DateTime.Now
        };

        _context.Expenditures.Add(expenditure);
        await _context.SaveChangesAsync(cancellationToken);

        return expenditure.ExpenditureID;
    }
}
