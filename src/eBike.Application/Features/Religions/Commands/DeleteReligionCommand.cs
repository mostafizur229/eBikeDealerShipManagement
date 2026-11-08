using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Religions.Commands;

public record DeleteReligionCommand(int Id) : IRequest;

public class DeleteReligionCommandHandler : IRequestHandler<DeleteReligionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteReligionCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteReligionCommand request, CancellationToken cancellationToken)
    {
        var religion = await _context.Religions
            .FirstOrDefaultAsync(r => r.ReligionID == request.Id, cancellationToken);

        if (religion == null)
        {
            throw new NotFoundException(nameof(Religion), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        var employeeCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Employees WHERE ReligionID = @id",
            new { id = request.Id });

        if (employeeCount > 0)
        {
            throw new ConflictException($"Cannot delete religion '{religion.Name}' because it is assigned to {employeeCount} employee(s).");
        }

        _context.Religions.Remove(religion);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
