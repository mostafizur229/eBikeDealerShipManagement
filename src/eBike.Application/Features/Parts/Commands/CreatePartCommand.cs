using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Parts.Commands;

public record CreatePartCommand(
    string? Code,
    string Description,
    int? ConcernId = null
) : IRequest<int>;

public class CreatePartCommandValidator : AbstractValidator<CreatePartCommand>
{
    public CreatePartCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Part Number / Description is required.")
            .MaximumLength(250).WithMessage("Part Number / Description must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");
    }
}

public class CreatePartCommandHandler : IRequestHandler<CreatePartCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ISender _sender;

    public CreatePartCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<int> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        int concernId = request.ConcernId ?? _currentUser.ConcernId
            ?? throw new UnauthorizedAccessException("Valid Concern ID is required to create a Part No.");

        var cleanDescription = request.Description.Trim();

        var isDuplicate = await _context.Parts
            .AnyAsync(p => p.Description != null && p.Description.ToLower() == cleanDescription.ToLower() && p.ConcernID == concernId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Part with the same Part No / Description already exists in the system.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextPartCodeQuery(concernId), cancellationToken);
        }

        var part = new Part
        {
            Code = code,
            Description = cleanDescription,
            ConcernID = concernId
        };

        _context.Parts.Add(part);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-replicate to connected family concerns
        var familyConcerns = await Common.Helpers.SisterConcernFamilyHelper
            .GetConnectedFamilyConcernsAsync(_context, concernId, cancellationToken);

        foreach (var familyConcern in familyConcerns)
        {
            var existsInOther = await _context.Parts
                .AnyAsync(p => p.ConcernID == familyConcern.ConcernID && p.Description != null && p.Description.ToLower() == cleanDescription.ToLower(), cancellationToken);

            if (!existsInOther)
            {
                var otherCode = await _sender.Send(new Queries.GetNextPartCodeQuery(familyConcern.ConcernID), cancellationToken);
                _context.Parts.Add(new Part
                {
                    Code = otherCode,
                    Description = cleanDescription,
                    ConcernID = familyConcern.ConcernID
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return part.PartsNoID;
    }
}
