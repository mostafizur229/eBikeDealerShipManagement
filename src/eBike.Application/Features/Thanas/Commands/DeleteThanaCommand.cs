using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Thanas.Commands;

public record DeleteThanaCommand(int Id) : IRequest;

public class DeleteThanaCommandHandler : IRequestHandler<DeleteThanaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteThanaCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteThanaCommand request, CancellationToken cancellationToken)
    {
        var thana = await _context.Thanas
            .FirstOrDefaultAsync(t => t.ThanaID == request.Id, cancellationToken);

        if (thana == null)
        {
            throw new NotFoundException(nameof(Thana), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        // Integrity check: Customers using this Thana
        var customerCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Customers WHERE ThanaID = @id",
            new { id = request.Id });

        if (customerCount > 0)
        {
            throw new ConflictException($"Cannot delete thana '{thana.Name}' because it is assigned to {customerCount} customer(s).");
        }

        // Integrity check: Unions under this Thana (if table exists)
        try
        {
            var unionCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Unions WHERE ThanaID = @id",
                new { id = request.Id });

            if (unionCount > 0)
            {
                throw new ConflictException($"Cannot delete thana '{thana.Name}' because it has {unionCount} associated union(s).");
            }
        }
        catch
        {
            // Table Unions might not exist or be used
        }

        _context.Thanas.Remove(thana);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
