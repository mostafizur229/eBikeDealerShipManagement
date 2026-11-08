using Dapper;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Employees.Commands;

public record DeleteEmployeeCommand(int Id) : IRequest;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public DeleteEmployeeCommandHandler(
        IApplicationDbContext context,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeID == request.Id, cancellationToken);

        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.Id);
        }

        using var connection = _sqlConnectionFactory.CreateConnection();

        var customerCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Customers WHERE EmployeeID = @id",
            new { id = request.Id });

        if (customerCount > 0)
        {
            throw new ConflictException($"Cannot delete employee '{employee.Name}' because they are assigned to {customerCount} customer(s).");
        }

        var userCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Users WHERE EmployeeID = @id",
            new { id = request.Id });

        if (userCount > 0)
        {
            throw new ConflictException($"Cannot delete employee '{employee.Name}' because an application user login account is linked to this employee.");
        }

        var payrollCount = await connection.ExecuteScalarAsync<int>(@"
            SELECT 
                (SELECT COUNT(1) FROM SalaryMonthlies WHERE EmployeeID = @id) +
                (SELECT COUNT(1) FROM AdvanceSalaries WHERE EmployeeID = @id)",
            new { id = request.Id });

        if (payrollCount > 0)
        {
            throw new ConflictException($"Cannot delete employee '{employee.Name}' because they have associated payroll/salary record(s).");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
