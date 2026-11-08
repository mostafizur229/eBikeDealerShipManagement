using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CashDeliveries.DTOs;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CashDeliveries.Commands;

public record UpdateCashDeliveryCommand(
    int Id,
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

public class UpdateCashDeliveryCommandValidator : AbstractValidator<UpdateCashDeliveryCommand>
{
    public UpdateCashDeliveryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cash Delivery ID must be greater than zero.");

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

public class UpdateCashDeliveryCommandHandler : IRequestHandler<UpdateCashDeliveryCommand, CashDeliveryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ICashDueService _cashDueService;

    public UpdateCashDeliveryCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ICashDueService cashDueService)
    {
        _context = context;
        _currentUser = currentUser;
        _cashDueService = cashDueService;
    }

    public async Task<CashDeliveryDto> Handle(UpdateCashDeliveryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CashCollections
            .FirstOrDefaultAsync(c => c.CashCollectionID == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Cash Delivery with ID {request.Id} not found.");

        if (_currentUser.ConcernId.HasValue && entity.ConcernID != _currentUser.ConcernId.Value)
        {
            throw new UnauthorizedAccessException("You are not authorized to update records outside your concern.");
        }

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierID == request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier with ID {request.SupplierId} not found.");

        decimal newAdjustAmt = request.AdjustAmt ?? 0m;
        decimal newTotalAmt = request.Amount + newAdjustAmt;

        if (entity.TransactionType == (int)EnumTranType.ToCompany)
        {
            await _cashDueService.UpdateTotalDueWhenEditAsync(0, request.SupplierId, entity.CashCollectionID, newTotalAmt, cancellationToken);
        }
        
        decimal currentSupplierDue = await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierID == request.SupplierId)
            .Select(s => s.TotalDue)
            .FirstOrDefaultAsync(cancellationToken);

        entity.SupplierID = request.SupplierId;
        entity.EntryDate = request.EntryDate;
        entity.Amount = request.Amount;
        entity.AdjustAmt = newAdjustAmt;
        entity.BalanceDue = currentSupplierDue;
        entity.PaymentType = request.PaymentType > 0 ? request.PaymentType : (int)EnumPayType.Cash;
        entity.AccountNo = string.IsNullOrWhiteSpace(request.AccountNo) ? "No A/C" : request.AccountNo.Trim();
        entity.BankName = request.BankName?.Trim();
        entity.BranchName = request.BranchName?.Trim();
        entity.MBAccountNo = request.MBAccountNo?.Trim();
        entity.BKashNo = request.BKashNo?.Trim();
        entity.Remarks = request.Remarks?.Trim();
        entity.ModifiedBy = _currentUser.UserId;
        entity.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CashDeliveryDto
        {
            CashCollectionID = entity.CashCollectionID,
            ReceiptNo = entity.ReceiptNo ?? string.Empty,
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
