using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using eBike.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Registrations.Commands;

public record DeleteRegistrationCommand(int Id) : IRequest<bool>;

public class DeleteRegistrationCommandHandler : IRequestHandler<DeleteRegistrationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteRegistrationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRegistrationCommand request, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .FirstOrDefaultAsync(r => r.RegistrationID == request.Id, cancellationToken);

        if (registration == null)
        {
            throw new NotFoundException(nameof(Registration), request.Id);
        }

        if (registration.Iscompleted.HasValue && registration.Iscompleted.Value != (int)EnumRegStatus.Initiate)
        {
            throw new InvalidOperationException("Cannot delete registration that has progressed beyond the Initiate status.");
        }

        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

