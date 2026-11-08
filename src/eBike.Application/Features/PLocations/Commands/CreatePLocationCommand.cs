using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.PLocations.Commands;

public record CreatePLocationCommand(
    string? Code,
    string Description,
    int? ConcernId = null
) : IRequest<int>;

public class CreatePLocationCommandValidator : AbstractValidator<CreatePLocationCommand>
{
    public CreatePLocationCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Rack / Location Description is required.")
            .MaximumLength(250).WithMessage("Rack / Location Description must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");
    }
}

public class CreatePLocationCommandHandler : IRequestHandler<CreatePLocationCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreatePLocationCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreatePLocationCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a Rack Location.");

        var cleanDescription = request.Description.Trim();

        var isDuplicate = await _context.PLocations
            .AnyAsync(p => p.Description != null && p.Description.ToLower() == cleanDescription.ToLower() && p.ConcernID == concernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Rack Location with the same name already exists in the system.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextPLocationCodeQuery(concernId), cancellationToken);
        }

        var location = new PLocation
        {
            Code = code,
            Description = cleanDescription,
            ConcernID = concernId
        };

        _context.PLocations.Add(location);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-replicate to connected family concerns
        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, concernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var existsInOther = await _context.PLocations
                .AnyAsync(p => p.ConcernID == familyConcern.ConcernID && p.Description != null && p.Description.ToLower() == cleanDescription.ToLower(), cancellationToken);

            if (!existsInOther)
            {
                var otherCode = await _sender.Send(new Queries.GetNextPLocationCodeQuery(familyConcern.ConcernID), cancellationToken);
                _context.PLocations.Add(new PLocation
                {
                    Code = otherCode,
                    Description = cleanDescription,
                    ConcernID = familyConcern.ConcernID
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return location.PLocationID;
    }
}
