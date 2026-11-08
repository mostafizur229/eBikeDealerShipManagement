using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Departments.Commands;

public record DeleteDepartmentCommand(int Id) : IRequest;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteDepartmentCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == request.Id, cancellationToken);

        if (department == null)
        {
            throw new NotFoundException(nameof(Department), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();
        var employeeCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Employees WHERE DepartmentID = @id",
            new { id = request.Id });

        if (employeeCount > 0)
        {
            throw new ConflictException($"Cannot delete department '{department.DESCRIPTION}' because it is assigned to {employeeCount} employee(s).");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
