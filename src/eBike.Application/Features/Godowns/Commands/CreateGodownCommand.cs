using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Godowns.Commands;

public record CreateGodownCommand(
    string? Code,
    string Name,
    string? Address,
    int? ConcernId,
    bool IsCommon = false
) : IRequest<int>;

public class CreateGodownCommandValidator : AbstractValidator<CreateGodownCommand>
{
    public CreateGodownCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Godown Name is required.");

        RuleFor(x => x.Code)
            .MaximumLength(250).WithMessage("Godown Code must not exceed 250 characters.");
    }
}

public class CreateGodownCommandHandler : IRequestHandler<CreateGodownCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateGodownCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateGodownCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId 
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a godown.");

        var isDuplicate = await _context.Godowns
            .AnyAsync(g => g.Name.ToLower() == request.Name.Trim().ToLower() && g.ConcernID == concernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Godown with same name already exists in the system. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextGodownCodeQuery(concernId), cancellationToken);
        }

        var godown = new Godown
        {
            Code = code,
            Name = request.Name.Trim(),
            Address = request.Address?.Trim(),
            ConcernID = concernId,
            ISCommon = request.IsCommon ? 1 : 0
        };

        _context.Godowns.Add(godown);
        await _context.SaveChangesAsync(cancellationToken);

        return godown.GodownID;
    }
}
