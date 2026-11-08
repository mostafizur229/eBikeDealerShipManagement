using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashCollections.DTOs;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Commands;

public record UpdateCashCollectionCommand(
    int Id,
    DateTime EntryDate,
    int CustomerId,
    decimal Amount,
    decimal? AdjustAmt,
    decimal? InterestAmt,
    int? PaymentType,
    int? TransactionType,
    string? AccountNo,
    string? BankName,
    string? BranchName,
    string? MBAccountNo,
    string? BKashNo,
    string? MRNo,
    string? Remarks,
    DateTime? RemindDate,
    int? ProjectType = null
) : IRequest<CashCollectionDto>;

public class UpdateCashCollectionCommandValidator : AbstractValidator<UpdateCashCollectionCommand>
{
    public UpdateCashCollectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cash Collection ID must be greater than zero.");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.AdjustAmt)
            .GreaterThanOrEqualTo(0).When(x => x.AdjustAmt.HasValue)
            .WithMessage("Adjust amount cannot be negative.");

        RuleFor(x => x.InterestAmt)
            .GreaterThanOrEqualTo(0).When(x => x.InterestAmt.HasValue)
            .WithMessage("Interest amount cannot be negative.");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("Entry date is required.");
    }
}

public class UpdateCashCollectionCommandHandler : IRequestHandler<UpdateCashCollectionCommand, CashCollectionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICashDueService _cashDueService;

    public UpdateCashCollectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICashDueService cashDueService)
    {
        _context = context;
        _currentUser = currentUser;
        _cashDueService = cashDueService;
    }

    public async Task<CashCollectionDto> Handle(UpdateCashCollectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CashCollections
            .FirstOrDefaultAsync(c => c.CashCollectionID == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cash Collection with ID {request.Id} not found.");

        if (_currentUser.ConcernId.HasValue && entity.ConcernID != _currentUser.ConcernId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to update records outside your concern.");
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");

        decimal newAdjustAmt = request.AdjustAmt ?? 0m;
        decimal newInterestAmt = request.InterestAmt ?? 0m;
        int tranType = request.TransactionType.HasValue && request.TransactionType.Value > 0
            ? request.TransactionType.Value
            : (entity.TransactionType ?? (int)EnumTranType.FromCustomer);

        decimal newNetCollectionAmount;
        if (tranType == (int)EnumTranType.CollectionReturn)
        {
            newNetCollectionAmount = -(request.Amount + newAdjustAmt);
        }
        else
        {
            newNetCollectionAmount = request.Amount + newAdjustAmt - newInterestAmt;
        }

        if (entity.TransactionType == (int)EnumTranType.FromCustomer ||
            entity.TransactionType == (int)EnumTranType.AdvanceCollection ||
            entity.TransactionType == (int)EnumTranType.CollectionReturn)
        {
            await _cashDueService.UpdateTotalDueWhenEditAsync(request.CustomerId, 0, entity.CashCollectionID, newNetCollectionAmount, cancellationToken);
        }
        
        decimal currentCustomerDue = await _context.Customers
            .AsNoTracking()
            .Where(c => c.CustomerID == request.CustomerId)
            .Select(c => c.TotalDue)
            .FirstOrDefaultAsync(cancellationToken);

        entity.CustomerID = request.CustomerId;
        entity.EntryDate = request.EntryDate;
        entity.Amount = request.Amount;
        entity.AdjustAmt = newAdjustAmt;
        entity.InterestAmt = newInterestAmt;
        entity.BalanceDue = currentCustomerDue;
        entity.PaymentType = request.PaymentType.HasValue && request.PaymentType.Value > 0
            ? request.PaymentType.Value
            : (int)EnumPayType.Cash;
        entity.TransactionType = tranType;
        entity.AccountNo = string.IsNullOrWhiteSpace(request.AccountNo) ? "No A/C" : request.AccountNo.Trim();
        entity.BankName = request.BankName?.Trim();
        entity.BranchName = request.BranchName?.Trim();
        entity.MBAccountNo = request.MBAccountNo?.Trim();
        entity.BKashNo = request.BKashNo?.Trim();
        entity.MRNo = request.MRNo?.Trim();
        entity.Remarks = request.Remarks?.Trim();
        entity.ModifiedBy = _currentUser.UserId;
        entity.ModifiedDate = DateTime.UtcNow;

        if (request.ProjectType.HasValue && request.ProjectType.Value > 0)
        {
            entity.ProjectType = request.ProjectType.Value;
        }

        if (request.RemindDate.HasValue && request.RemindDate.Value > request.EntryDate)
        {
            customer.RemindDate = request.RemindDate.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CashCollectionDto
        {
            CashCollectionID = entity.CashCollectionID,
            ReceiptNo = entity.ReceiptNo ?? string.Empty,
            EntryDate = entity.EntryDate,
            CustomerID = entity.CustomerID,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            CustomerContactNo = customer.ContactNo,
            Amount = entity.Amount ?? 0m,
            AdjustAmt = entity.AdjustAmt,
            InterestAmt = entity.InterestAmt,
            BalanceDue = entity.BalanceDue,
            PaymentType = entity.PaymentType ?? 1,
            AccountNo = entity.AccountNo,
            BankName = entity.BankName,
            BranchName = entity.BranchName,
            MBAccountNo = entity.MBAccountNo,
            BKashNo = entity.BKashNo,
            MRNo = entity.MRNo,
            Remarks = entity.Remarks,
            RemindDate = customer.RemindDate,
            TransactionType = entity.TransactionType ?? (int)EnumTranType.FromCustomer,
            ProjectType = entity.ProjectType,
            ConcernID = entity.ConcernID,
            CreatedBy = entity.CreatedBy,
            CreateDate = entity.CreateDate
        };
    }
}
