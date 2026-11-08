using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PLocations.Commands;

public record UpdatePLocationCommand(
    int Id,
    string Code,
    string Description,
    int? ConcernId = null
) : IRequest;

public class UpdatePLocationCommandValidator : AbstractValidator<UpdatePLocationCommand>
{
    public UpdatePLocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Location ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Rack / Location Description is required.")
            .MaximumLength(250).WithMessage("Rack / Location Description must not exceed 250 characters.");
    }
}

public class UpdatePLocationCommandHandler : IRequestHandler<UpdatePLocationCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePLocationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _context.PLocations
            .FirstOrDefaultAsync(p => p.PLocationID == request.Id, cancellationToken);

        if (location == null)
        {
            throw new NotFoundException(nameof(PLocation), request.Id);
        }

        int targetConcernId = request.ConcernId ?? location.ConcernID;
        var cleanDescription = request.Description.Trim();
        var oldDescription = location.Description?.Trim();

        var isDuplicate = await _context.PLocations
            .AnyAsync(p => p.PLocationID != request.Id &&
                           p.Description != null &&
                           p.Description.ToLower() == cleanDescription.ToLower() &&
                           p.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("Another Rack Location with the same name already exists in this concern.");
        }

        location.Code = request.Code.Trim();
        location.Description = cleanDescription;
        location.ConcernID = targetConcernId;

        // Auto-replicate update to matching locations in connected family concerns
        if (!string.IsNullOrWhiteSpace(oldDescription))
        {
            var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
                .GetConnectedFamilyConcernsAsync(_context, targetConcernId, cancellationToken);

            foreach (var familyConcern in familyConcerns)
            {
                var otherLocation = await _context.PLocations
                    .FirstOrDefaultAsync(p => p.ConcernID == familyConcern.ConcernID &&
                                              p.Description != null &&
                                              p.Description.ToLower() == oldDescription.ToLower(), cancellationToken);

                if (otherLocation != null)
                {
                    otherLocation.Description = cleanDescription;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
