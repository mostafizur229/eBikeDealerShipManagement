using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Expenditures.Commands;

public record DeleteExpenditureCommand(int Id) : IRequest;

public class DeleteExpenditureCommandHandler : IRequestHandler<DeleteExpenditureCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteExpenditureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteExpenditureCommand request, CancellationToken cancellationToken)
    {
        var expenditure = await _context.Expenditures
            .FirstOrDefaultAsync(e => e.ExpenditureID == request.Id, cancellationToken);

        if (expenditure == null)
        {
            throw new NotFoundException(nameof(Expenditure), request.Id);
        }

        _context.Expenditures.Remove(expenditure);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
