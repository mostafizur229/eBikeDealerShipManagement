using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record SendToFileProcessingCommand(
    int Id,
    DateTime FileReceivedDate
) : IRequest<RegistrationDto>;

public class SendToFileProcessingCommandHandler : IRequestHandler<SendToFileProcessingCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public SendToFileProcessingCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<RegistrationDto> Handle(SendToFileProcessingCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.RegistrationID == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        registration.FileReceivedDate = request.FileReceivedDate;
        registration.Iscompleted = (int)EnumRegStatus.FileProcessing;
        registration.ModifiedDate = DateTime.Now;
        registration.ModifiedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetRegistrationByIdQuery(registration.RegistrationID), cancellationToken);
    }
}

