using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Commands;

public record UpdateThanaCommand(
    int ThanaID,
    string Code,
    string Name,
    int DistrictId
) : IRequest;

public class UpdateThanaCommandValidator : AbstractValidator<UpdateThanaCommand>
{
    public UpdateThanaCommandValidator()
    {
        RuleFor(x => x.ThanaID)
            .GreaterThan(0).WithMessage("Valid Thana ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Thana Name is required.")
            .MaximumLength(250).WithMessage("Thana Name must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Thana Code is required.")
            .MaximumLength(250).WithMessage("Thana Code must not exceed 250 characters.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("District is required.");
    }
}

public class UpdateThanaCommandHandler : IRequestHandler<UpdateThanaCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateThanaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateThanaCommand request, CancellationToken cancellationToken)
    {
        var thana = await _context.Thanas
            .FirstOrDefaultAsync(t => t.ThanaID == request.ThanaID, cancellationToken);

        if (thana == null)
        {
            throw new NotFoundException(nameof(Thana), request.ThanaID);
        }

        var districtExists = await _context.Districts
            .AnyAsync(d => d.DistrictID == request.DistrictId, cancellationToken);

        if (!districtExists)
        {
            throw new NotFoundException(nameof(District), request.DistrictId);
        }

        var isDuplicate = await _context.Thanas
            .AnyAsync(t => t.ThanaID != request.ThanaID &&
                           t.DistrictID == request.DistrictId &&
                           t.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Thana with same name already exists in this district. Please try with a different name.");
        }

        thana.Code = request.Code.Trim();
        thana.Name = request.Name.Trim();
        thana.DistrictID = request.DistrictId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
