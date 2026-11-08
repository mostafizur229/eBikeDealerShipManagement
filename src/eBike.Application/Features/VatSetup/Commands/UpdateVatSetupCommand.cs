using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.VatSetup.Commands;

public record UpdateVatSetupCommand(
    int? ConcernId,
    decimal SalesShowPercent,
    decimal PurchaseShowPercent,
    decimal StockShowPercent
) : IRequest<bool>;

public class UpdateVatSetupCommandValidator : AbstractValidator<UpdateVatSetupCommand>
{
    public UpdateVatSetupCommandValidator()
    {
        RuleFor(x => x.SalesShowPercent)
            .InclusiveBetween(0, 100).WithMessage("Sales Show Percent must be between 0 and 100.");

        RuleFor(x => x.PurchaseShowPercent)
            .InclusiveBetween(0, 100).WithMessage("Purchase Show Percent must be between 0 and 100.");

        RuleFor(x => x.StockShowPercent)
            .InclusiveBetween(0, 100).WithMessage("Stock Show Percent must be between 0 and 100.");
    }
}

public class UpdateVatSetupCommandHandler : IRequestHandler<UpdateVatSetupCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public UpdateVatSetupCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(UpdateVatSetupCommand request, CancellationToken cancellationToken)
    {
        var targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var concern = await _context.SisterConcerns
            .Where(sc => targetConcernId == null || targetConcernId == 0 || sc.ConcernID == targetConcernId)
            .OrderBy(sc => sc.ConcernID)
            .FirstOrDefaultAsync(cancellationToken);

        if (concern == null)
        {
            throw new NotFoundException("SisterConcern", targetConcernId?.ToString() ?? "Default");
        }

        concern.SalesShowPercent = request.SalesShowPercent;
        concern.PurchaseShowPercent = request.PurchaseShowPercent;
        concern.StockShowPercent = request.StockShowPercent;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
