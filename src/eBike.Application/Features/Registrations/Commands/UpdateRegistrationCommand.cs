using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record UpdateRegistrationCommand(
    int Id,
    DateTime RegApplyDate,
    int RegType,
    decimal RegFee,
    decimal InsuranceAmt = 0,
    string? REGMRNo = null
) : IRequest<RegistrationDto>;

public class UpdateRegistrationCommandHandler : IRequestHandler<UpdateRegistrationCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public UpdateRegistrationCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<RegistrationDto> Handle(UpdateRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.RegistrationID == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        if (registration.Iscompleted.HasValue && registration.Iscompleted.Value != (int)EnumRegStatus.Initiate)
        {
            throw new InvalidOperationException("Only registrations in Initiate status can be edited directly.");
        }

        registration.RegApplyDate = request.RegApplyDate;
        registration.RegType = request.RegType;
        registration.RegFee = request.RegFee;
        registration.InsuranceAmt = request.InsuranceAmt;
        registration.REGMRNo = request.REGMRNo;
        registration.ModifiedDate = DateTime.Now;
        registration.ModifiedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetRegistrationByIdQuery(registration.RegistrationID), cancellationToken);
    }
}

