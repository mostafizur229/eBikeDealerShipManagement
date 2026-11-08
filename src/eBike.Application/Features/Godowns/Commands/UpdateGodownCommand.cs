using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Godowns.Commands;

public record UpdateGodownCommand(
    int Id,
    string Code,
    string Name,
    string? Address,
    int? ConcernId,
    bool IsCommon = false
) : IRequest;

public class UpdateGodownCommandValidator : AbstractValidator<UpdateGodownCommand>
{
    public UpdateGodownCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid Godown ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Godown Code is required.")
            .MaximumLength(250).WithMessage("Godown Code must not exceed 250 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Godown Name is required.");
    }
}

public class UpdateGodownCommandHandler : IRequestHandler<UpdateGodownCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateGodownCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateGodownCommand request, CancellationToken cancellationToken)
    {
        var godown = await _context.Godowns
            .FirstOrDefaultAsync(g => g.GodownID == request.Id, cancellationToken);

        if (godown == null)
        {
            throw new NotFoundException(nameof(Godown), request.Id);
        }

        int targetConcernId = request.ConcernId ?? godown.ConcernID;

        var isDuplicate = await _context.Godowns
            .AnyAsync(g => g.GodownID != request.Id &&
                           g.Name.ToLower() == request.Name.Trim().ToLower() &&
                           g.ConcernID == targetConcernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Godown with same name already exists in the system. Please try with a different name.");
        }

        godown.Code = request.Code.Trim();
        godown.Name = request.Name.Trim();
        godown.Address = request.Address?.Trim();
        godown.ConcernID = targetConcernId;
        godown.ISCommon = request.IsCommon ? 1 : 0;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
