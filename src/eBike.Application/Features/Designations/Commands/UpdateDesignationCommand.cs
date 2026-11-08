using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Designations.Commands;

public record UpdateDesignationCommand(
    int DesignationID,
    string Code,
    string Description,
    decimal? CommissionPercent = null
) : IRequest;

public class UpdateDesignationCommandValidator : AbstractValidator<UpdateDesignationCommand>
{
    public UpdateDesignationCommandValidator()
    {
        RuleFor(x => x.DesignationID)
            .GreaterThan(0).WithMessage("Valid Designation ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Designation Name is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Designation Code is required.")
            .MaximumLength(150).WithMessage("Designation Code must not exceed 150 characters.");
    }
}

public class UpdateDesignationCommandHandler : IRequestHandler<UpdateDesignationCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDesignationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDesignationCommand request, CancellationToken cancellationToken)
    {
        var designation = await _context.Designations
            .FirstOrDefaultAsync(d => d.DesignationID == request.DesignationID, cancellationToken);

        if (designation == null)
        {
            throw new NotFoundException(nameof(Designation), request.DesignationID);
        }

        var isDuplicate = await _context.Designations
            .AnyAsync(d => d.DesignationID != request.DesignationID &&
                           d.Description.ToLower() == request.Description.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Designation with same name already exists in the system. Please try with a different name.");
        }

        designation.Code = request.Code.Trim();
        designation.Description = request.Description.Trim();
        designation.CommissionPercent = request.CommissionPercent;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
