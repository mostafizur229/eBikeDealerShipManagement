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

public record UpdateServiceTicketStatusCommand(int TicketId, ServiceStatus Status, string? WorkNotes = null) : IRequest<bool>;

public class UpdateServiceTicketStatusCommandHandler : IRequestHandler<UpdateServiceTicketStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceTicketStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateServiceTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.ServiceTickets
            .Include(t => t.EBike)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            throw new NotFoundException(nameof(ServiceTicket), request.TicketId);
        }

        ticket.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.WorkNotes))
        {
            ticket.WorkNotes = string.IsNullOrWhiteSpace(ticket.WorkNotes)
                ? request.WorkNotes
                : $"{ticket.WorkNotes}\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {request.WorkNotes}";
        }

        if (request.Status == ServiceStatus.Completed && !ticket.CompletedDate.HasValue)
        {
            ticket.CompletedDate = DateTime.UtcNow;
        }
        else if (request.Status == ServiceStatus.Delivered)
        {
            ticket.DeliveredDate = DateTime.UtcNow;
            if (ticket.EBike != null)
            {
                ticket.EBike.Status = EBikeStatus.Available;
                ticket.EBike.CurrentOdometerKm = ticket.CurrentMileageKm;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
