using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.ReplacementOrders.DTOs;
using eBike.Application.Features.ReplacementOrders.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.ReplacementOrders.Commands;

public record CreateReplacementOrderCommand(
    string? InvoiceNo,
    DateTime? InvoiceDate,
    int CustomerId,
    decimal AdjAmount,
    decimal RecAmount,
    string? Remarks,
    int? ConcernId,
    List<CreateReplacementOrderItemInput> Items
) : IRequest<ReplacementOrderDto>;

public class CreateReplacementOrderItemInput
{
    public int DamageStockDetailId { get; set; }
    public int DamageProductId { get; set; }
    public decimal DamageUnitPrice { get; set; }
    public int ReplaceProductType { get; set; } = 1; // 1 = Damage, 2 = NoDamage
    public int NewStockDetailId { get; set; }
    public int NewProductId { get; set; }
    public decimal NewUnitPrice { get; set; }
    public string? Remarks { get; set; }
}

public class CreateReplacementOrderCommandValidator : AbstractValidator<CreateReplacementOrderCommand>
{
    public CreateReplacementOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one product must be replaced.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.DamageStockDetailId)
                .GreaterThan(0).WithMessage("Original (damage) vehicle is required.");

            item.RuleFor(i => i.NewStockDetailId)
                .GreaterThan(0).WithMessage("New replacement vehicle is required.");

            item.RuleFor(i => i.ReplaceProductType)
                .Must(t => t == 1 || t == 2).WithMessage("Replacement type must be 1 (Damage) or 2 (No Damage).");
        });
    }
}

public class CreateReplacementOrderCommandHandler : IRequestHandler<CreateReplacementOrderCommand, ReplacementOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public CreateReplacementOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<ReplacementOrderDto> Handle(CreateReplacementOrderCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {request.CustomerId} not found.");

        string invoiceNo = string.IsNullOrWhiteSpace(request.InvoiceNo)
            ? await _mediator.Send(new GetNextReplacementInvoiceNoQuery(targetConcernId), cancellationToken)
            : request.InvoiceNo.Trim();

        decimal damageTotal = request.Items.Sum(i => i.DamageUnitPrice);
        decimal replaceTotal = request.Items.Sum(i => i.NewUnitPrice);
        decimal grandTotal = replaceTotal - damageTotal;
        decimal totalAmount = grandTotal + request.AdjAmount;
        decimal paymentDue = totalAmount - request.RecAmount;

        // Update customer balance: difference between new product and damage product minus customer payment
        customer.TotalDue += paymentDue;

        var sOrder = new SOrder
        {
            InvoiceNo = invoiceNo,
            InvoiceDate = request.InvoiceDate ?? DateTime.Today,
            CustomerID = request.CustomerId,
            GrandTotal = grandTotal,
            AdjAmount = request.AdjAmount,
            TotalAmount = totalAmount,
            PaymentDue = paymentDue,
            RecAmount = request.RecAmount,
            TotalDue = customer.TotalDue,
            Status = (int)EnumSalesType.Sales,
            IsReplacement = 1,
            ConcernID = targetConcernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now,
            Remarks = request.Remarks
        };

        _context.SOrders.Add(sOrder);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            // 1. Validate & update original sold item's SOrderDetail
            var originalSod = await _context.SOrderDetails
                .Include(s => s.SOrder)
                .FirstOrDefaultAsync(s => s.SDetailID == item.DamageStockDetailId
                                          && s.SOrder != null
                                          && s.SOrder.CustomerID == request.CustomerId
                                          && s.RepOrderID == null, cancellationToken);

            if (originalSod != null)
            {
                originalSod.RepOrderID = sOrder.SOrderID;
                originalSod.RStockDetailId = item.NewStockDetailId;
                originalSod.RepUnitPrice = item.NewUnitPrice;
                originalSod.ReplaceProductType = item.ReplaceProductType;
                originalSod.Remarks = item.Remarks;
            }

            // 2. Update damaged/old item's StockDetail
            var damageStockDetail = await _context.StockDetails
                .Include(sd => sd.Stock)
                .FirstOrDefaultAsync(sd => sd.SDetailID == item.DamageStockDetailId, cancellationToken);

            if (damageStockDetail != null)
            {
                if (item.ReplaceProductType == 1) // Damage
                {
                    damageStockDetail.Status = (int)EnumStockStatus.Damage;
                }
                else // NoDamage / Good condition
                {
                    damageStockDetail.Status = (int)EnumStockStatus.Stock;
                    if (damageStockDetail.Stock != null)
                    {
                        damageStockDetail.Stock.Quantity += 1;
                    }
                }
            }

            // 3. Insert new replacement item's SOrderDetail
            var newSod = new SOrderDetail
            {
                SOrderID = sOrder.SOrderID,
                ProductID = item.NewProductId,
                Quantity = 1,
                UnitPrice = item.NewUnitPrice,
                UTAmount = item.NewUnitPrice,
                SDetailID = item.NewStockDetailId,
                Remarks = item.Remarks,
                ReplaceProductType = item.ReplaceProductType
            };
            _context.SOrderDetails.Add(newSod);

            // 4. Update new item's StockDetail to Sold & decrement stock
            var newStockDetail = await _context.StockDetails
                .Include(sd => sd.Stock)
                .FirstOrDefaultAsync(sd => sd.SDetailID == item.NewStockDetailId, cancellationToken);

            if (newStockDetail != null)
            {
                newStockDetail.Status = (int)EnumStockStatus.Sold;
                if (newStockDetail.Stock != null && newStockDetail.Stock.Quantity > 0)
                {
                    newStockDetail.Stock.Quantity -= 1;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetReplacementOrderByIdQuery(sOrder.SOrderID), cancellationToken);
    }
}
