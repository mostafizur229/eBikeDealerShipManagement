using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Colors.Commands;

public record UpdateColorCommand(
    int Id,
    string Code,
    string Name,
    int? ConcernId
) : IRequest;

public class UpdateColorCommandValidator : AbstractValidator<UpdateColorCommand>
{
    public UpdateColorCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Color ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Color Code is required.")
            .MaximumLength(250).WithMessage("Color Code must not exceed 250 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Color Name is required.")
            .MaximumLength(250).WithMessage("Color Name must not exceed 250 characters.");
    }
}

public class UpdateColorCommandHandler : IRequestHandler<UpdateColorCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateColorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateColorCommand request, CancellationToken cancellationToken)
    {
        var color = await _context.Colors
            .FirstOrDefaultAsync(c => c.ColorID == request.Id, cancellationToken);

        if (color == null)
        {
            throw new NotFoundException(nameof(Color), request.Id);
        }

        int targetConcernId = request.ConcernId ?? color.ConcernID;

        var isDuplicate = await _context.Colors
            .AnyAsync(c => c.ColorID != request.Id &&
                           c.Name.ToLower() == request.Name.Trim().ToLower() &&
                           c.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Color with same name already exists in the system. Please try with a different name.");
        }

        var oldName = color.Name.Trim();
        var newName = request.Name.Trim();

        color.Code = request.Code.Trim();
        color.Name = newName;
        color.ConcernID = targetConcernId;

        // Auto-replicate update to connected family concerns where color name matches
        if (!string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
        {
            var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
                .GetConnectedFamilyConcernsAsync(_context, color.ConcernID, cancellationToken);
            var familyConcernIds = familyConcerns.Select(f => f.ConcernID).ToList();

            var matchingOtherColors = await _context.Colors
                .Where(c => familyConcernIds.Contains(c.ConcernID) && c.Name.ToLower() == oldName.ToLower())
                .ToListAsync(cancellationToken);

            foreach (var other in matchingOtherColors)
            {
                other.Name = newName;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
