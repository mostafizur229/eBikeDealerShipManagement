using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashCollections.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashCollections.Commands;

public record CreateCashCollectionCommand(
    string? ReceiptNo,
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
    int ProjectType = 1
) : IRequest<CashCollectionDto>;

public class CreateCashCollectionCommandValidator : AbstractValidator<CreateCashCollectionCommand>
{
    public CreateCashCollectionCommandValidator()
    {
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

public class CreateCashCollectionCommandHandler : IRequestHandler<CreateCashCollectionCommand, CashCollectionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICashDueService _cashDueService;

    public CreateCashCollectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICashDueService cashDueService)
    {
        _context = context;
        _currentUser = currentUser;
        _cashDueService = cashDueService;
    }

    public async Task<CashCollectionDto> Handle(CreateCashCollectionCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = _currentUser.ConcernId ?? 0;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");

        if (targetConcernId == 0)
        {
            targetConcernId = customer.ConcernID;
        }

        // Generate or validate ReceiptNo (shares the same sequential number pool in CashCollections)
        string receiptNo = request.ReceiptNo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(receiptNo))
        {
            var existingReceipts = await _context.CashCollections
                .AsNoTracking()
                .Where(c => c.ConcernID == targetConcernId && c.ReceiptNo != null)
                .Select(c => c.ReceiptNo!)
                .ToListAsync(cancellationToken);

            int maxNo = 0;
            foreach (var r in existingReceipts)
            {
                if (int.TryParse(r, out int parsed) && parsed > maxNo)
                {
                    maxNo = parsed;
                }
            }
            receiptNo = (maxNo + 1).ToString("D5");
        }
        else
        {
            bool exists = await _context.CashCollections
                .AnyAsync(c => c.ConcernID == targetConcernId && c.ReceiptNo == receiptNo, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Receipt No '{receiptNo}' already exists.");
            }
        }

        decimal adjustAmt = request.AdjustAmt ?? 0m;
        decimal interestAmt = request.InterestAmt ?? 0m;
        int tranType = request.TransactionType.HasValue && request.TransactionType.Value > 0
            ? request.TransactionType.Value
            : (int)EnumTranType.FromCustomer;

        decimal netCollectionAmount;
        if (tranType == (int)EnumTranType.CollectionReturn)
        {
            netCollectionAmount = -(request.Amount + adjustAmt);
        }
        else
        {
            netCollectionAmount = request.Amount + adjustAmt - interestAmt;
        }

        decimal balanceDue = customer.TotalDue - netCollectionAmount;
        int projectType = request.ProjectType > 0 ? request.ProjectType : 1;

        var entity = new CashCollection
        {
            ReceiptNo = receiptNo,
            EntryDate = request.EntryDate,
            CustomerID = request.CustomerId,
            SupplierID = 0,
            Amount = request.Amount,
            AdjustAmt = adjustAmt,
            InterestAmt = interestAmt,
            BalanceDue = balanceDue,
            PaymentType = request.PaymentType.HasValue && request.PaymentType.Value > 0
                ? request.PaymentType.Value
                : (int)EnumPayType.Cash,
            AccountNo = string.IsNullOrWhiteSpace(request.AccountNo) ? "No A/C" : request.AccountNo.Trim(),
            BankName = request.BankName?.Trim(),
            BranchName = request.BranchName?.Trim(),
            MBAccountNo = request.MBAccountNo?.Trim(),
            BKashNo = request.BKashNo?.Trim(),
            MRNo = request.MRNo?.Trim(),
            Remarks = request.Remarks?.Trim(),
            TransactionType = tranType,
            ConcernID = targetConcernId,
            CreatedBy = _currentUser.UserId ?? 1,
            CreateDate = DateTime.UtcNow,
            ProjectType = projectType
        };

        if (request.RemindDate.HasValue && request.RemindDate.Value > request.EntryDate)
        {
            customer.RemindDate = request.RemindDate.Value;
        }

        _context.CashCollections.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _cashDueService.UpdateTotalDueAsync(request.CustomerId, 0, netCollectionAmount, cancellationToken);

        return new CashCollectionDto
        {
            CashCollectionID = entity.CashCollectionID,
            ReceiptNo = entity.ReceiptNo,
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
