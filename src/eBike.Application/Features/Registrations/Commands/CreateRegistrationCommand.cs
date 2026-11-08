using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record CreateRegistrationCommand(
    int CustomerId,
    int? StockDetailId,
    int? SOrderDetailId,
    int? CreditSaleDetailsId,
    DateTime RegApplyDate,
    int RegType,
    decimal RegFee,
    decimal InsuranceAmt = 0,
    string? REGMRNo = null
) : IRequest<RegistrationDto>;

public class CreateRegistrationCommandHandler : IRequestHandler<CreateRegistrationCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public CreateRegistrationCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<RegistrationDto> Handle(CreateRegistrationCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        int? sOrderDetailId = request.SOrderDetailId;
        int? creditSaleDetailsId = request.CreditSaleDetailsId;
        int productId = 0;
        DateTime? salesDate = null;
        int concernId = _currentUser.ConcernId ?? customer.ConcernID;


        if (!sOrderDetailId.HasValue && !creditSaleDetailsId.HasValue && request.StockDetailId.HasValue)
        {
            var cashDetail = await (
                from sod in _context.SOrderDetails
                join so in _context.SOrders on sod.SOrderID equals so.SOrderID
                where (sod.RStockDetailId ?? sod.SDetailID) == request.StockDetailId.Value && sod.IsProductReturn == 0
                select new { sod.SOrderDetailID, sod.ProductID, so.InvoiceDate, so.ConcernID }
            ).FirstOrDefaultAsync(cancellationToken);

            if (cashDetail != null)
            {
                sOrderDetailId = cashDetail.SOrderDetailID;
                productId = cashDetail.ProductID;
                salesDate = cashDetail.InvoiceDate;
                concernId = cashDetail.ConcernID;
            }
            else
            {
                var creditDetail = await (
                    from csd in _context.CreditSaleDetails
                    join cs in _context.CreditSales on csd.CreditSalesID equals cs.CreditSalesID
                    where csd.StockDetailID == request.StockDetailId.Value && csd.IsProductReturn == 0
                    select new { csd.CreditSaleDetailsID, csd.ProductID, cs.SalesDate, cs.ConcernID }
                ).FirstOrDefaultAsync(cancellationToken);

                if (creditDetail != null)
                {
                    creditSaleDetailsId = creditDetail.CreditSaleDetailsID;
                    productId = creditDetail.ProductID;
                    salesDate = creditDetail.SalesDate;
                    concernId = creditDetail.ConcernID;
                }
            }
        }
        else if (sOrderDetailId.HasValue)
        {
            var cashDetail = await (
                from sod in _context.SOrderDetails
                join so in _context.SOrders on sod.SOrderID equals so.SOrderID
                where sod.SOrderDetailID == sOrderDetailId.Value
                select new { sod.ProductID, so.InvoiceDate, so.ConcernID }
            ).FirstOrDefaultAsync(cancellationToken);

            if (cashDetail != null)
            {
                productId = cashDetail.ProductID;
                salesDate = cashDetail.InvoiceDate;
                concernId = cashDetail.ConcernID;
            }
        }
        else if (creditSaleDetailsId.HasValue)
        {
            var creditDetail = await (
                from csd in _context.CreditSaleDetails
                join cs in _context.CreditSales on csd.CreditSalesID equals cs.CreditSalesID
                where csd.CreditSaleDetailsID == creditSaleDetailsId.Value
                select new { csd.ProductID, cs.SalesDate, cs.ConcernID }
            ).FirstOrDefaultAsync(cancellationToken);

            if (creditDetail != null)
            {
                productId = creditDetail.ProductID;
                salesDate = creditDetail.SalesDate;
                concernId = creditDetail.ConcernID;
            }
        }

        if (productId == 0)
        {
            throw new BadRequestException("Sold product not found for this customer.");
        }

        bool alreadyRegistered = false;
        if (sOrderDetailId.HasValue)
        {
            alreadyRegistered = await _context.Registrations
                .AnyAsync(r => r.SorderDetailID == sOrderDetailId.Value, cancellationToken);
        }
        else if (creditSaleDetailsId.HasValue)
        {
            alreadyRegistered = await _context.Registrations
                .AnyAsync(r => r.CreditSaleDetailsID == creditSaleDetailsId.Value, cancellationToken);
        }

        if (alreadyRegistered)
        {
            throw new ConflictException("This product is already registered.");
        }

        var registration = new Registration
        {
            ConcernID = concernId,
            CustomerID = request.CustomerId,
            ProductID = productId,
            SorderDetailID = sOrderDetailId,
            CreditSaleDetailsID = creditSaleDetailsId,
            SalesDate = salesDate,
            RegApplyDate = request.RegApplyDate,
            RegType = request.RegType,
            RegFee = request.RegFee,
            InsuranceAmt = request.InsuranceAmt,
            REGMRNo = request.REGMRNo,
            Iscompleted = (int)EnumRegStatus.Initiate,
            WFStatus = (int)EnumWFStatus.Approved,
            CreateDate = DateTime.Now,
            CreatedBy = _currentUser.UserId ?? 0
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetRegistrationByIdQuery(registration.RegistrationID), cancellationToken);
    }
}