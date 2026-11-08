using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CreditInterestPercentages.Queries;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditInterestPercentages.Commands;

public record CreateCreditInterestPercentageCommand(
    string? Code,
    decimal Percentage,
    DateTime? EffectDate,
    int? ConcernId = null
) : IRequest<int>;

public class CreateCreditInterestPercentageCommandValidator : AbstractValidator<CreateCreditInterestPercentageCommand>
{
    public CreateCreditInterestPercentageCommandValidator()
    {
        RuleFor(x => x.Percentage)
            .GreaterThanOrEqualTo(0).WithMessage("Percentage must be greater than or equal to 0.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");
    }
}

public class CreateCreditInterestPercentageCommandHandler : IRequestHandler<CreateCreditInterestPercentageCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateCreditInterestPercentageCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateCreditInterestPercentageCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a Credit Interest Percentage.");

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await _sender.Send(new GetNextCreditInterestCodeQuery(concernId), cancellationToken)
            : request.Code.Trim();

        var exists = await _context.CreditInterestPercentages
            .AnyAsync(c => c.ConcernID == concernId && c.Percentage == request.Percentage, cancellationToken);

        if (exists)
        {
            throw new ConflictException("A Credit Interest Percentage with same percentage already exists in the system.");
        }

        var entity = new CreditInterestPercentage
        {
            Code = code,
            Percentage = request.Percentage,
            EffectDate = request.EffectDate ?? DateTime.Today,
            ConcernID = concernId
        };

        _context.CreditInterestPercentages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.IntPercentageID;
    }
}
