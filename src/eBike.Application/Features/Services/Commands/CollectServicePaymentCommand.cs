using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record CollectServicePaymentCommand(
    int TicketId,
    decimal Amount,
    decimal? UpdatedLaborCost,
    decimal? UpdatedDiscount,
    string PaymentMethod,
    string? Remarks
) : IRequest<decimal>;

public class CollectServicePaymentCommandHandler : IRequestHandler<CollectServicePaymentCommand, decimal>
{
    private readonly IApplicationDbContext _context;

    public CollectServicePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> Handle(CollectServicePaymentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.ServiceTickets
            .Include(t => t.UsedParts)
            .Include(t => t.EBike)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException(nameof(ServiceTicket), request.TicketId);
        }

        if (request.UpdatedLaborCost.HasValue)
        {
            ticket.LaborCost = request.UpdatedLaborCost.Value;
        }

        if (request.UpdatedDiscount.HasValue)
        {
            ticket.Discount = request.UpdatedDiscount.Value;
        }

        ticket.PaidAmount += request.Amount;

        // Note payment in worknotes
        var note = $"[Payment Received] ৳{request.Amount:N2} via {request.PaymentMethod}. Total Paid: ৳{ticket.PaidAmount:N2}. Remarks: {request.Remarks ?? "N/A"}";
        ticket.WorkNotes = string.IsNullOrWhiteSpace(ticket.WorkNotes)
            ? note
            : $"{ticket.WorkNotes}\n{note}";

        if (ticket.PaidAmount >= ticket.TotalAmount && ticket.Status < ServiceStatus.Completed)
        {
            ticket.Status = ServiceStatus.Completed;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ticket.PaidAmount;
    }
}
