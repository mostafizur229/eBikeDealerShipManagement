using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Commands;

public record CreateThanaCommand(
    string? Code,
    string Name,
    int DistrictId
) : IRequest<int>;

public class CreateThanaCommandValidator : AbstractValidator<CreateThanaCommand>
{
    public CreateThanaCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Thana Name is required.")
            .MaximumLength(250).WithMessage("Thana Name must not exceed 250 characters.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("District is required.");

        RuleFor(x => x.Code)
            .MaximumLength(250).WithMessage("Thana Code must not exceed 250 characters.");
    }
}

public class CreateThanaCommandHandler : IRequestHandler<CreateThanaCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public CreateThanaCommandHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<int> Handle(CreateThanaCommand request, CancellationToken cancellationToken)
    {
        var districtExists = await _context.Districts
            .AnyAsync(d => d.DistrictID == request.DistrictId, cancellationToken);

        if (!districtExists)
        {
            throw new NotFoundException(nameof(District), request.DistrictId);
        }

        var isDuplicate = await _context.Thanas
            .AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower() && t.DistrictID == request.DistrictId, cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Thana with same name already exists in this district. Please try with a different name.");
        }

        string code = request.Code?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await _sender.Send(new Queries.GetNextThanaCodeQuery(), cancellationToken);
        }

        var thana = new Thana
        {
            Code = code,
            Name = request.Name.Trim(),
            DistrictID = request.DistrictId
        };

        _context.Thanas.Add(thana);
        await _context.SaveChangesAsync(cancellationToken);

        return thana.ThanaID;
    }
}
