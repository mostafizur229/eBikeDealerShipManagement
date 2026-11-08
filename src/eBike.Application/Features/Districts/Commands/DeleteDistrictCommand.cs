using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Districts.Commands;

public record DeleteDistrictCommand(int Id) : IRequest;

public class DeleteDistrictCommandHandler : IRequestHandler<DeleteDistrictCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteDistrictCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteDistrictCommand request, CancellationToken cancellationToken)
    {
        var district = await _context.Districts
            .FirstOrDefaultAsync(d => d.DistrictID == request.Id, cancellationToken);

        if (district == null)
        {
            throw new NotFoundException(nameof(District), request.Id);
        }

        // Integrity check: Thanas under this District
        var hasThanas = await _context.Thanas
            .AnyAsync(t => t.DistrictID == request.Id, cancellationToken);

        if (hasThanas)
        {
            throw new ConflictException($"Cannot delete district '{district.Name}' because it has associated Thana record(s).");
        }

        // Integrity check: Customers using this District
        using var connection = _sqlConnectionFactory.CreateConnection();
        var customerCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Customers WHERE DistrictID = @id",
            new { id = request.Id });

        if (customerCount > 0)
        {
            throw new ConflictException($"Cannot delete district '{district.Name}' because it is assigned to {customerCount} customer(s).");
        }

        _context.Districts.Remove(district);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
