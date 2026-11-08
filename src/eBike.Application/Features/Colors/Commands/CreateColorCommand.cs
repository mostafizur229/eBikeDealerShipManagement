using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Colors.Commands;

public record CreateColorCommand(
    string? Code,
    string Name,
    int? ConcernId
) : IRequest<int>;

public class CreateColorCommandValidator : AbstractValidator<CreateColorCommand>
{
    public CreateColorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Color Name is required.")
            .MaximumLength(250).WithMessage("Color Name must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(250).WithMessage("Color Code must not exceed 250 characters.");
    }
}

public class CreateColorCommandHandler : IRequestHandler<CreateColorCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreateColorCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreateColorCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId 
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a color.");

        var isDuplicate = await _context.Colors
            .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && c.ConcernID == concernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Color with same name already exists in the system. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextColorCodeQuery(concernId), cancellationToken);
        }

        var color = new Color
        {
            Code = code,
            Name = request.Name.Trim(),
            ConcernID = concernId
        };

        _context.Colors.Add(color);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-replicate to connected family concerns (matching MCSRMIMSWEB GetFamilyTree)
        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, concernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var existsInOther = await _context.Colors
                .AnyAsync(c => c.ConcernID == familyConcern.ConcernID && c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

            if (!existsInOther)
            {
                var otherCode = await _sender.Send(new Queries.GetNextColorCodeQuery(familyConcern.ConcernID), cancellationToken);
                _context.Colors.Add(new Color
                {
                    Code = otherCode,
                    Name = request.Name.Trim(),
                    ConcernID = familyConcern.ConcernID
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return color.ColorID;
    }
}
