using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Parts.Commands;

public record UpdatePartCommand(
    int Id,
    string Code,
    string Description,
    int? ConcernId = null
) : IRequest;

public class UpdatePartCommandValidator : AbstractValidator<UpdatePartCommand>
{
    public UpdatePartCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Part ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Part Number / Description is required.")
            .MaximumLength(250).WithMessage("Part Number / Description must not exceed 250 characters.");
    }
}

public class UpdatePartCommandHandler : IRequestHandler<UpdatePartCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _context.Parts
            .FirstOrDefaultAsync(p => p.PartsNoID == request.Id, cancellationToken);

        if (part == null)
        {
            throw new NotFoundException(nameof(Part), request.Id);
        }

        int targetConcernId = request.ConcernId ?? part.ConcernID;
        var cleanDescription = request.Description.Trim();
        var oldDescription = part.Description?.Trim();

        var isDuplicate = await _context.Parts
            .AnyAsync(p => p.PartsNoID != request.Id &&
                           p.Description != null &&
                           p.Description.ToLower() == cleanDescription.ToLower() &&
                           p.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("Another Part with the same Part No / Description already exists in this concern.");
        }

        part.Code = request.Code.Trim();
        part.Description = cleanDescription;
        part.ConcernID = targetConcernId;

        // Auto-replicate update to matching parts in connected family concerns
        if (!string.IsNullOrWhiteSpace(oldDescription))
        {
            var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
                .GetConnectedFamilyConcernsAsync(_context, targetConcernId, cancellationToken);

            foreach (var familyConcern in familyConcerns)
            {
                var otherPart = await _context.Parts
                    .FirstOrDefaultAsync(p => p.ConcernID == familyConcern.ConcernID &&
                                              p.Description != null &&
                                              p.Description.ToLower() == oldDescription.ToLower(), cancellationToken);

                if (otherPart != null)
                {
                    otherPart.Description = cleanDescription;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
