using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Godowns.Commands;

public record DeleteGodownCommand(int Id) : IRequest;

public class DeleteGodownCommandHandler : IRequestHandler<DeleteGodownCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteGodownCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteGodownCommand request, CancellationToken cancellationToken)
    {
        var godown = await _context.Godowns
            .FirstOrDefaultAsync(g => g.GodownID == request.Id, cancellationToken);

        if (godown == null)
        {
            throw new NotFoundException(nameof(Godown), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        int usageCount = await connection.ExecuteScalarAsync<int>(@"
            SELECT 
                (SELECT COUNT(1) FROM Stocks WHERE GodownID = @id) +
                (SELECT COUNT(1) FROM StockDetails WHERE GodownID = @id) +
                (SELECT COUNT(1) FROM POrderDetails WHERE GodownID = @id);",
            new { id = request.Id });

        if (usageCount > 0)
        {
            throw new ConflictException($"Cannot delete godown '{godown.Name}' because it has {usageCount} active Stock or Purchase record(s).");
        }

        _context.Godowns.Remove(godown);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
