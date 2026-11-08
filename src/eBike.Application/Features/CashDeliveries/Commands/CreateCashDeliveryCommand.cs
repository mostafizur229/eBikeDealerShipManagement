using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashDeliveries.DTOs;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashDeliveries.Commands;

public record CreateCashDeliveryCommand(
    string? ReceiptNo,
    DateTime EntryDate,
    int SupplierId,
    decimal Amount,
    decimal? AdjustAmt,
    int PaymentType,
    string? AccountNo,
    string? BankName,
    string? BranchName,
    string? MBAccountNo,
    string? BKashNo,
    string? Remarks
) : IRequest<CashDeliveryDto>;

public class CreateCashDeliveryCommandValidator : AbstractValidator<CreateCashDeliveryCommand>
{
    public CreateCashDeliveryCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.AdjustAmt)
            .GreaterThanOrEqualTo(0).When(x => x.AdjustAmt.HasValue)
            .WithMessage("Adjust amount cannot be negative.");

        RuleFor(x => x.EntryDate)
            .NotEmpty().WithMessage("Entry date is required.");
    }
}

public class CreateCashDeliveryCommandHandler : IRequestHandler<CreateCashDeliveryCommand, CashDeliveryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICashDueService _cashDueService;

    public CreateCashDeliveryCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICashDueService cashDueService)
    {
        _context = context;
        _currentUser = currentUser;
        _cashDueService = cashDueService;
    }

    public async Task<CashDeliveryDto> Handle(CreateCashDeliveryCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = _currentUser.ConcernId ?? 0;

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier with ID {request.SupplierId} not found.");

        if (targetConcernId == 0)
        {
            targetConcernId = supplier.ConcernID;
        }

        // Generate or validate ReceiptNo
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
        decimal totalAmt = request.Amount + adjustAmt;
        decimal balanceDue = supplier.TotalDue - totalAmt;

        var entity = new CashCollection
        {
            ReceiptNo = receiptNo,
            EntryDate = request.EntryDate,
            SupplierID = request.SupplierId,
            CustomerID = 0,
            Amount = request.Amount,
            AdjustAmt = adjustAmt,
            BalanceDue = balanceDue,
            PaymentType = request.PaymentType > 0 ? request.PaymentType : (int)EnumPayType.Cash,
            AccountNo = string.IsNullOrWhiteSpace(request.AccountNo) ? "No A/C" : request.AccountNo.Trim(),
            BankName = request.BankName?.Trim(),
            BranchName = request.BranchName?.Trim(),
            MBAccountNo = request.MBAccountNo?.Trim(),
            BKashNo = request.BKashNo?.Trim(),
            Remarks = request.Remarks?.Trim(),
            TransactionType = (int)EnumTranType.ToCompany,
            ConcernID = targetConcernId,
            CreatedBy = _currentUser.UserId ?? 1,
            CreateDate = DateTime.UtcNow,
            InterestAmt = 0m,
            ProjectType = 1
        };

        _context.CashCollections.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Update supplier balance via Stored Procedure
        await _cashDueService.UpdateTotalDueAsync(0, request.SupplierId, totalAmt, cancellationToken);

        return new CashDeliveryDto
        {
            CashCollectionID = entity.CashCollectionID,
            ReceiptNo = entity.ReceiptNo,
            EntryDate = entity.EntryDate,
            SupplierID = entity.SupplierID,
            SupplierName = supplier.Name,
            SupplierCode = supplier.Code,
            Amount = entity.Amount ?? 0m,
            AdjustAmt = entity.AdjustAmt,
            BalanceDue = entity.BalanceDue,
            PaymentType = entity.PaymentType ?? 1,
            AccountNo = entity.AccountNo,
            BankName = entity.BankName,
            BranchName = entity.BranchName,
            MBAccountNo = entity.MBAccountNo,
            BKashNo = entity.BKashNo,
            Remarks = entity.Remarks,
            TransactionType = entity.TransactionType ?? (int)EnumTranType.ToCompany,
            ConcernID = entity.ConcernID,
            CreatedBy = entity.CreatedBy,
            CreateDate = entity.CreateDate
        };
    }
}
