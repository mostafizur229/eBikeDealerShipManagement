using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Districts.Commands;

public record CreateDistrictCommand(
    string? Code,
    string Name
) : IRequest<int>;

public class CreateDistrictCommandValidator : AbstractValidator<CreateDistrictCommand>
{
    public CreateDistrictCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("District Name is required.")
            .MaximumLength(250).WithMessage("District Name must not exceed 250 characters.");

        RuleFor(x => x.Code)
            .MaximumLength(250).WithMessage("District Code must not exceed 250 characters.");
    }
}

public class CreateDistrictCommandHandler : IRequestHandler<CreateDistrictCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public CreateDistrictCommandHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<int> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        var isDuplicate = await _context.Districts
            .AnyAsync(d => d.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A District with same name already exists in the system. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextDistrictCodeQuery(), cancellationToken);
        }

        var district = new District
        {
            Code = code,
            Name = request.Name.Trim()
        };

        _context.Districts.Add(district);
        await _context.SaveChangesAsync(cancellationToken);

        return district.DistrictID;
    }
}
