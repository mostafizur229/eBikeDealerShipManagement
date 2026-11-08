using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.SalesReturns.DTOs;
using eBike.Application.Features.SalesReturns.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.SalesReturns.Commands;

public record CreateSalesReturnCommand(
    DateTime? ReturnDate,
    string? InvoiceNo,
    int CustomerId,
    decimal? RecAmount,
    string? Remarks,
    int? ConcernId,
    List<CreateSalesReturnItemInput> Items
) : IRequest<SalesReturnDto>;

public class CreateSalesReturnItemInput
{
    public int ProductId { get; set; }
    public int? StockDetailId { get; set; }
    public int? ColorId { get; set; }
    public int GodownId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmt { get; set; }
    public string? IMENO { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
}

public class CreateSalesReturnCommandValidator : AbstractValidator<CreateSalesReturnCommand>
{
    public CreateSalesReturnCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item must be returned.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Product is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Return quantity must be greater than zero.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        });
    }
}

public class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, SalesReturnDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public CreateSalesReturnCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<SalesReturnDto> Handle(CreateSalesReturnCommand request, CancellationToken cancellationToken)
    {
        int targetConcernId = request.ConcernId ?? _currentUser.ConcernId ?? 0;
        int userId = _currentUser.UserId ?? 0;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken);
        if (customer == null)
            throw new NotFoundException($"Customer with ID {request.CustomerId} not found.");

        string invoiceNo = string.IsNullOrWhiteSpace(request.InvoiceNo)
            ? await _mediator.Send(new GetNextSalesReturnInvoiceNoQuery(targetConcernId), cancellationToken)
            : request.InvoiceNo.Trim();

        decimal grandTotal = request.Items.Sum(i => i.TotalAmt > 0 ? i.TotalAmt : (i.Quantity * i.UnitPrice));
        decimal recAmount = request.RecAmount ?? 0;
        decimal paymentDue = grandTotal - recAmount;
        decimal totalDue = customer.TotalDue - paymentDue;

        var sOrder = new SOrder
        {
            InvoiceNo = invoiceNo,
            InvoiceDate = request.ReturnDate ?? DateTime.Today,
            CustomerID = request.CustomerId,
            GrandTotal = grandTotal,
            TotalAmount = grandTotal,
            RecAmount = recAmount,
            PaymentDue = paymentDue,
            TotalDue = totalDue,
            Status = (int)EnumSalesType.ProductReturn,
            SOType = 1,
            ConcernID = targetConcernId,
            CreatedBy = userId,
            CreateDate = DateTime.Now,
            Remarks = request.Remarks,
            VATPercentage = 0,
            VATAmount = 0,
            TDPercentage = 0,
            TDAmount = 0,
            AdjAmount = 0,
            TotalOffer = 0,
            PGrandTotal = 0,
            IsReplacement = 0,
            NetDiscount = 0
        };

        _context.SOrders.Add(sOrder);
        await _context.SaveChangesAsync(cancellationToken);

        customer.TotalDue = totalDue;

        foreach (var item in request.Items)
        {
            var sOrderDetail = new SOrderDetail
            {
                SOrderID = sOrder.SOrderID,
                ProductID = item.ProductId,
                SDetailID = item.StockDetailId ?? 0,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                UTAmount = item.TotalAmt > 0 ? item.TotalAmt : (item.Quantity * item.UnitPrice),
                IsProductReturn = 1,
                MPRate = item.UnitPrice,
                SRate = item.UnitPrice
            };

            _context.SOrderDetails.Add(sOrderDetail);

            if (item.StockDetailId.HasValue && item.StockDetailId.Value > 0)
            {
                var stockDetail = await _context.StockDetails
                    .FirstOrDefaultAsync(sd => sd.SDetailID == item.StockDetailId.Value, cancellationToken);

                if (stockDetail != null)
                {
                    stockDetail.Status = (int)EnumStockStatus.Stock; // Restored to active inventory!
                    stockDetail.Quantity = item.Quantity;
                }

                var parentStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductID == item.ProductId &&
                                              s.GodownID == item.GodownId &&
                                              (item.ColorId == null || s.ColorID == item.ColorId) &&
                                              s.ConcernID == targetConcernId, cancellationToken);

                if (parentStock != null)
                {
                    parentStock.Quantity += item.Quantity;
                    parentStock.ModifiedDate = DateTime.Now;
                    parentStock.ModifiedBy = userId;
                }
            }
            else
            {
                var parentStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductID == item.ProductId &&
                                              s.GodownID == item.GodownId &&
                                              s.ConcernID == targetConcernId, cancellationToken);

                if (parentStock != null)
                {
                    parentStock.Quantity += item.Quantity;
                    parentStock.ModifiedDate = DateTime.Now;
                    parentStock.ModifiedBy = userId;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = await _mediator.Send(new GetSalesReturnByIdQuery(sOrder.SOrderID), cancellationToken);
        return resultDto!;
    }
}
