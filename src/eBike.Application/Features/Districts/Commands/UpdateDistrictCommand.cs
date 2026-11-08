using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Districts.Commands;

public record UpdateDistrictCommand(
    int DistrictID,
    string Code,
    string Name
) : IRequest;

public class UpdateDistrictCommandValidator : AbstractValidator<UpdateDistrictCommand>
{
    public UpdateDistrictCommandValidator()
    {
        RuleFor(x => x.DistrictID)
            .GreaterThan(0).WithMessage("Valid District ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("District Name is required.")
            .MaximumLength(250).WithMessage("District Name must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("District Code is required.")
            .MaximumLength(250).WithMessage("District Code must not exceed 250 characters.");
    }
}

public class UpdateDistrictCommandHandler : IRequestHandler<UpdateDistrictCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDistrictCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = await _context.Districts
            .FirstOrDefaultAsync(d => d.DistrictID == request.DistrictID, cancellationToken);

        if (district == null)
        {
            throw new NotFoundException(nameof(District), request.DistrictID);
        }

        var isDuplicate = await _context.Districts
            .AnyAsync(d => d.DistrictID != request.DistrictID &&
                           d.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A District with same name already exists in the system. Please try with a different name.");
        }

        district.Code = request.Code.Trim();
        district.Name = request.Name.Trim();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
