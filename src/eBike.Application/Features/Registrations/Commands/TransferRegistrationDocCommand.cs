using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record TransferRegistrationDocCommand(
    int Id,
    int NewCustomerId,
    DateTime TransferDate
) : IRequest<RegistrationDto>;

public class TransferRegistrationDocCommandHandler : IRequestHandler<TransferRegistrationDocCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public TransferRegistrationDocCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<RegistrationDto> Handle(TransferRegistrationDocCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.RegistrationID == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        if (registration.IsDocTransfer == 1)
        {
            throw new InvalidOperationException("This registration document has already been transferred.");
        }

        var newCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerID == request.NewCustomerId, cancellationToken);
        if (newCustomer == null)
        {
            throw new NotFoundException(nameof(Customer), request.NewCustomerId);
        }

        registration.NewCusID = request.NewCustomerId;
        registration.TransferDate = request.TransferDate;
        registration.IsDocTransfer = 1;
        registration.ModifiedDate = DateTime.Now;
        registration.ModifiedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetRegistrationByIdQuery(registration.RegistrationID), cancellationToken);
    }
}

