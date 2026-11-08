using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Colors.Commands;

public record DeleteColorCommand(int Id) : IRequest;

public class DeleteColorCommandHandler : IRequestHandler<DeleteColorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteColorCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteColorCommand request, CancellationToken cancellationToken)
    {
        var color = await _context.Colors
            .FirstOrDefaultAsync(c => c.ColorID == request.Id, cancellationToken);

        if (color == null)
        {
            throw new NotFoundException(nameof(Color), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        int usageCount = await connection.ExecuteScalarAsync<int>(@"
            SELECT 
                (SELECT COUNT(1) FROM StockDetails WHERE ColorID = @id) +
                (SELECT COUNT(1) FROM Stocks WHERE ColorID = @id) +
                (SELECT COUNT(1) FROM POrderDetails WHERE ColorID = @id) +
                (SELECT COUNT(1) FROM CustomerVehicles WHERE ColorID = @id);",
            new { id = request.Id });

        if (usageCount > 0)
        {
            throw new ConflictException($"Cannot delete color '{color.Name}' because it is used in {usageCount} active Purchase, Sales, or Stock transaction(s).");
        }

        _context.Colors.Remove(color);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
