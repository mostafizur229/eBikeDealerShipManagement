using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Designations.Commands;

public record DeleteDesignationCommand(int Id) : IRequest;

public class DeleteDesignationCommandHandler : IRequestHandler<DeleteDesignationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteDesignationCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteDesignationCommand request, CancellationToken cancellationToken)
    {
        var designation = await _context.Designations
            .FirstOrDefaultAsync(d => d.DesignationID == request.Id, cancellationToken);

        if (designation == null)
        {
            throw new NotFoundException(nameof(Designation), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        var employeeCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Employees WHERE DesignationID = @id",
            new { id = request.Id });

        if (employeeCount > 0)
        {
            throw new ConflictException($"Cannot delete designation '{designation.Description}' because it is assigned to {employeeCount} employee(s).");
        }

        _context.Designations.Remove(designation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
