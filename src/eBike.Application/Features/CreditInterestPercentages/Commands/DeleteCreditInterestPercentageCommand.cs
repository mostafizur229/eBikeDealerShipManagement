using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CreditInterestPercentages.Commands;

public record DeleteCreditInterestPercentageCommand(int Id) : IRequest<bool>;

public class DeleteCreditInterestPercentageCommandHandler : IRequestHandler<DeleteCreditInterestPercentageCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCreditInterestPercentageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCreditInterestPercentageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CreditInterestPercentages
            .FirstOrDefaultAsync(p => p.IntPercentageID == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Credit Interest Percentage with ID {request.Id} was not found.");

        _context.CreditInterestPercentages.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
