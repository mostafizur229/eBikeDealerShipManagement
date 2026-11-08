using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PurchaseDOs.Commands;

public record UpdatePurchaseDOCommand(
    int DOID,
    DateTime Date,
    string DONo,
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
) : IRequest<bool>;

public class UpdatePurchaseDOCommandValidator : AbstractValidator<UpdatePurchaseDOCommand>
{
    public UpdatePurchaseDOCommandValidator()
    {
        RuleFor(x => x.DOID).GreaterThan(0).WithMessage("Valid DO ID is required.");
        RuleFor(x => x.DONo).NotEmpty().WithMessage("DO No is required.");
        RuleFor(x => x.SupplierId).GreaterThan(0).WithMessage("Supplier is required.");
        RuleFor(x => x.Details).NotEmpty().WithMessage("At least one product item is required.");

        RuleForEach(x => x.Details).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Valid Product is required.");
            item.RuleFor(i => i.DoQty).GreaterThan(0).WithMessage("DO Quantity must be greater than 0.");
        });
    }
}

public class UpdatePurchaseDOCommandHandler : IRequestHandler<UpdatePurchaseDOCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdatePurchaseDOCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdatePurchaseDOCommand request, CancellationToken cancellationToken)
    {
        var purchaseDO = await _context.DOes
            .Include(d => d.DODetails)
            .FirstOrDefaultAsync(d => d.DOID == request.DOID, cancellationToken)
            ?? throw new NotFoundException($"Purchase DO with ID {request.DOID} was not found.");

        int userId = _currentUser.UserId ?? 1;
        int concernId = request.ConcernId ?? _currentUser.ConcernId ?? purchaseDO.ConcernID;

        purchaseDO.Date = request.Date;
        purchaseDO.DONo = request.DONo.Trim();
        purchaseDO.CustomerID = request.CustomerId;
        purchaseDO.SupplierID = request.SupplierId;
        purchaseDO.Status = request.Status;
        purchaseDO.Remarks = request.Remarks?.Trim();
        purchaseDO.TotalAmt = request.TotalAmt;
        purchaseDO.PaidAmt = request.PaidAmt;
        purchaseDO.NetDiscount = request.NetDiscount;
        purchaseDO.TotalTax = request.TotalTax;
        purchaseDO.GrandTotal = request.GrandTotal;
        purchaseDO.ConcernID = concernId;
        purchaseDO.ModifiedBy = userId;
        purchaseDO.ModifiedDate = DateTime.UtcNow;

        // Remove deleted items
        var requestDetailIds = request.Details.Where(d => d.DodId.HasValue && d.DodId.Value > 0).Select(d => d.DodId!.Value).ToHashSet();
        foreach (var existingDetail in purchaseDO.DODetails.ToList())
        {
            if (!requestDetailIds.Contains(existingDetail.DODID))
            {
                _context.DODetails.Remove(existingDetail);
            }
        }

        // Update existing or add new items
        foreach (var item in request.Details)
        {
            if (item.DodId.HasValue && item.DodId.Value > 0)
            {
                var existingDetail = purchaseDO.DODetails.FirstOrDefault(d => d.DODID == item.DodId.Value);
                if (existingDetail != null)
                {
                    existingDetail.ProductID = item.ProductId;
                    existingDetail.ColorID = item.ColorId;
                    existingDetail.DOQty = item.DoQty;
                    existingDetail.GivenQty = item.GivenQty;
                    existingDetail.MRP = item.Mrp;
                    existingDetail.UnitPrice = item.UnitPrice;
                    existingDetail.TotalAmt = item.TotalAmt;
                    existingDetail.PPDisPercent = item.PpDisPercent;
                    existingDetail.PPDisAmt = item.PpDisAmt;
                    existingDetail.PPTaxAmount = item.PpTaxAmount;
                }
            }
            else
            {
                purchaseDO.DODetails.Add(new PurchaseDODetail
                {
                    DOID = purchaseDO.DOID,
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
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
