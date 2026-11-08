using System.Globalization;
using System.Text.RegularExpressions;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Registrations.DTOs;
using eBike.Application.Features.Registrations.Queries;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record SendToObservationCommand(
    int Id,
    DateTime ObservationDate,
    decimal? BRTATIPS = null,
    decimal? BRTARegFee = null
) : IRequest<RegistrationDto>;

public class SendToObservationCommandHandler : IRequestHandler<SendToObservationCommand, RegistrationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMediator _mediator;

    public SendToObservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<RegistrationDto> Handle(SendToObservationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.RegistrationID == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        registration.ObservationDate = request.ObservationDate;
        registration.BRTATIPS = request.BRTATIPS ?? 0;
        registration.Iscompleted = (int)EnumRegStatus.Observation;
        registration.ModifiedDate = DateTime.Now;
        registration.ModifiedBy = _currentUser.UserId;

        if (request.BRTARegFee.HasValue && request.BRTARegFee.Value > 0)
        {
            registration.BRTARegFee = request.BRTARegFee.Value;
        }
        else
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == registration.ProductID, cancellationToken);
            decimal cc = product != null ? ParseCC(product.CC) : 0;

            var brtaRate = await _context.BRTARates
                .Where(r => r.RegType == registration.RegType)
                .OrderBy(r => r.ID)
                .ToListAsync(cancellationToken);

            if (brtaRate.Count > 0)
            {
                var matched = cc <= 100
                    ? brtaRate.FirstOrDefault(r => r.CC <= 100)
                    : brtaRate.FirstOrDefault(r => r.CC > 100);

                matched ??= brtaRate.FirstOrDefault();
                if (matched != null)
                {
                    registration.BRTARegFee = matched.TotalAmount;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetRegistrationByIdQuery(registration.RegistrationID), cancellationToken);
    }

    private static decimal ParseCC(string? ccStr)
    {
        if (string.IsNullOrWhiteSpace(ccStr)) return 0;
        var match = Regex.Match(ccStr, @"\d+(\.\d+)?");
        if (match.Success && decimal.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var cc))
        {
            return cc;
        }
        return 0;
    }
}

