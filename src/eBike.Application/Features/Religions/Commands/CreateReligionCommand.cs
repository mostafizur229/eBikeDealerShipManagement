using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Religions.Commands;

public record CreateReligionCommand(
    string Code,
    string Name,
    int SequenceNO = 1,
    int Status = 1
) : IRequest<int>;

public class CreateReligionCommandValidator : AbstractValidator<CreateReligionCommand>
{
    public CreateReligionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Religion Name is required.")
            .MaximumLength(100).WithMessage("Religion Name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Religion Code is required.")
            .MaximumLength(10).WithMessage("Religion Code must not exceed 10 characters.");
    }
}

public class CreateReligionCommandHandler : IRequestHandler<CreateReligionCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateReligionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateReligionCommand request, CancellationToken cancellationToken)
    {
        var isDuplicate = await _context.Religions
            .AnyAsync(r => r.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Religion with same name already exists in the system. Please try with a different name.");
        }

        var religion = new Religion
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            SequenceNO = request.SequenceNO,
            Status = request.Status,
            CreateDate = DateTime.Now
        };

        _context.Religions.Add(religion);
        await _context.SaveChangesAsync(cancellationToken);

        return religion.ReligionID;
    }
}
