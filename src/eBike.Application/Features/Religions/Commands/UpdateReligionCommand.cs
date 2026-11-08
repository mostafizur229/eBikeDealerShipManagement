using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Religions.Commands;

public record UpdateReligionCommand(
    int ReligionID,
    string Code,
    string Name,
    int SequenceNO = 1,
    int Status = 1
) : IRequest;

public class UpdateReligionCommandValidator : AbstractValidator<UpdateReligionCommand>
{
    public UpdateReligionCommandValidator()
    {
        RuleFor(x => x.ReligionID)
            .GreaterThan(0).WithMessage("Valid Religion ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Religion Name is required.")
            .MaximumLength(100).WithMessage("Religion Name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Religion Code is required.")
            .MaximumLength(10).WithMessage("Religion Code must not exceed 10 characters.");
    }
}

public class UpdateReligionCommandHandler : IRequestHandler<UpdateReligionCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateReligionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateReligionCommand request, CancellationToken cancellationToken)
    {
        var religion = await _context.Religions
            .FirstOrDefaultAsync(r => r.ReligionID == request.ReligionID, cancellationToken);

        if (religion == null)
        {
            throw new NotFoundException(nameof(Religion), request.ReligionID);
        }

        var isDuplicate = await _context.Religions
            .AnyAsync(r => r.ReligionID != request.ReligionID &&
                           r.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictException("A Religion with same name already exists in the system. Please try with a different name.");
        }

        religion.Code = request.Code.Trim();
        religion.Name = request.Name.Trim();
        religion.SequenceNO = request.SequenceNO;
        religion.Status = request.Status;
        religion.ModifiedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
