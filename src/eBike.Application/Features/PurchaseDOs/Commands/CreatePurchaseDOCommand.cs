using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.PurchaseDOs.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseDOs.Commands;

public record PurchaseDOItemCommand(
    int? DodId,
    int ProductId,
    int ColorId,
    decimal DoQty,
    decimal GivenQty,
    decimal Mrp,
    decimal UnitPrice,
    decimal TotalAmt,
    decimal PpDisPercent,
    decimal PpDisAmt,
    decimal PpTaxAmount
);

public record CreatePurchaseDOCommand(
    DateTime Date,
    string? DONo,
    int CustomerId,
    int SupplierId,
    int Status,
    string? Remarks,
    decimal TotalAmt,
    decimal PaidAmt,
    decimal NetDiscount,
    decimal TotalTax,
    decimal GrandTotal,
    int? ConcernId,
    List<PurchaseDOItemCommand> Details
) : IRequest<int>;

public class CreatePurchaseDOCommandValidator : AbstractValidator<CreatePurchaseDOCommand>
{
    public CreatePurchaseDOCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("Supplier is required.");

        RuleFor(x => x.Details)
            .NotEmpty().WithMessage("At least one product item is required.");

        RuleForEach(x => x.Details).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Valid Product is required.");
            item.RuleFor(i => i.DoQty).GreaterThan(0).WithMessage("DO Quantity must be greater than 0.");
        });
    }
}

public class CreatePurchaseDOCommandHandler : IRequestHandler<CreatePurchaseDOCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreatePurchaseDOCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreatePurchaseDOCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a Purchase DO.");

        int userId = _currentUser.UserId ?? 1;

        var doNo = string.IsNullOrWhiteSpace(request.DONo)
            ? await _sender.Send(new GetNextDONoQuery(concernId), cancellationToken)
            : request.DONo.Trim();

        var purchaseDO = new PurchaseDO
        {
            Date = request.Date,
            DONo = doNo,
            CustomerID = request.CustomerId,
            SupplierID = request.SupplierId,
            Status = request.Status > 0 ? request.Status : 1,
            Remarks = request.Remarks?.Trim(),
            TotalAmt = request.TotalAmt,
            PaidAmt = request.PaidAmt,
            NetDiscount = request.NetDiscount,
            TotalTax = request.TotalTax,
            GrandTotal = request.GrandTotal,
            ConcernID = concernId,
            CreatedBy = userId,
            CreateDate = DateTime.UtcNow
        };

        foreach (var item in request.Details)
        {
            purchaseDO.DODetails.Add(new PurchaseDODetail
            {
                ProductID = item.ProductId,
                ColorID = item.ColorId,
                DOQty = item.DoQty,
                GivenQty = item.GivenQty,
                MRP = item.Mrp,
                UnitPrice = item.UnitPrice,
                TotalAmt = item.TotalAmt,
                PPDisPercent = item.PpDisPercent,
                PPDisAmt = item.PpDisAmt,
                PPTaxAmount = item.PpTaxAmount
            });
        }

        _context.DOes.Add(purchaseDO);
        await _context.SaveChangesAsync(cancellationToken);

        return purchaseDO.DOID;
    }
}
