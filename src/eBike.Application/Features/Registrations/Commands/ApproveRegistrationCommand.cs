using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record ApproveRegistrationCommand(int Id) : IRequest<RegistrationDto>;

public class ApproveRegistrationCommandHandler : IRequestHandler<ApproveRegistrationCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public ApproveRegistrationCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<RegistrationDto> Handle(ApproveRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.RegistrationID == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        if (registration.WFStatus != (int)EnumWFStatus.Pending)
        {
            throw new InvalidOperationException("Registration is not in pending status.");
        }

        registration.WFStatus = (int)EnumWFStatus.Approved;
        registration.ModifiedDate = DateTime.Now;
        registration.ModifiedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetRegistrationByIdQuery(registration.RegistrationID), cancellationToken);
    }
}

