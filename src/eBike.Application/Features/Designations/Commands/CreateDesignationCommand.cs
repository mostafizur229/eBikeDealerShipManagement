using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Designations.Commands;

public record CreateDesignationCommand(
    string? Code,
    string Description,
    decimal? CommissionPercent = null
) : IRequest<int>;

public class CreateDesignationCommandValidator : AbstractValidator<CreateDesignationCommand>
{
    public CreateDesignationCommandValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Designation Name is required.");

        RuleFor(x => x.Code)
            .MaximumLength(150).WithMessage("Designation Code must not exceed 150 characters.");
    }
}

public class CreateDesignationCommandHandler : IRequestHandler<CreateDesignationCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public CreateDesignationCommandHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<int> Handle(CreateDesignationCommand request, CancellationToken cancellationToken)
    {
        var isDuplicate = await _context.Designations
            .AnyAsync(d => d.Description.ToLower() == request.Description.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Designation with same name already exists in the system. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextDesignationCodeQuery(), cancellationToken);
        }

        var designation = new Designation
        {
            Code = code,
            Description = request.Description.Trim(),
            CommissionPercent = request.CommissionPercent
        };

        _context.Designations.Add(designation);
        await _context.SaveChangesAsync(cancellationToken);

        return designation.DesignationID;
    }
}
