using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditInterestPercentages.Commands;

public record UpdateCreditInterestPercentageCommand(
    int IntPercentageId,
    string Code,
    decimal Percentage,
    DateTime? EffectDate,
    int? ConcernId = null
) : IRequest<bool>;

public class UpdateCreditInterestPercentageCommandValidator : AbstractValidator<UpdateCreditInterestPercentageCommand>
{
    public UpdateCreditInterestPercentageCommandValidator()
    {
        RuleFor(x => x.IntPercentageId)
            .GreaterThan(0).WithMessage("Valid Credit Interest Percentage ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Percentage)
            .GreaterThanOrEqualTo(0).WithMessage("Percentage must be greater than or equal to 0.");
    }
}

public class UpdateCreditInterestPercentageCommandHandler : IRequestHandler<UpdateCreditInterestPercentageCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateCreditInterestPercentageCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdateCreditInterestPercentageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CreditInterestPercentages
            .FirstOrDefaultAsync(p => p.IntPercentageID == request.IntPercentageId, cancellationToken)
            ?? throw new NotFoundException($"Credit Interest Percentage with ID {request.IntPercentageId} was not found.");

        int concernId = request.ConcernId ?? _currentUser.ConcernId ?? entity.ConcernID;

        var duplicate = await _context.CreditInterestPercentages
            .AnyAsync(c => c.IntPercentageID != request.IntPercentageId
                           && c.ConcernID == concernId
                           && c.Percentage == request.Percentage,
                      cancellationToken);

        if (duplicate)
        {
            throw new ConflictException("Another Credit Interest Percentage with the same percentage already exists in the system.");
        }

        entity.Code = request.Code.Trim();
        entity.Percentage = request.Percentage;
        entity.EffectDate = request.EffectDate ?? entity.EffectDate;
        entity.ConcernID = concernId;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
