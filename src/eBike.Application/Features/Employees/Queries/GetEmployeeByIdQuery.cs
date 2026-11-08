using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Employees.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Employees.Queries;

public record GetEmployeeByIdQuery(int Id) : IRequest<EmployeeDto>;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Designation)
            .Include(e => e.Department)
            .Include(e => e.Religion)
            .Include(e => e.SisterConcern)
            .FirstOrDefaultAsync(e => e.EmployeeID == request.Id, cancellationToken);

        if (employee == null)
        {
            throw new NotFoundException(nameof(Employee), request.Id);
        }

        return new EmployeeDto
        {
            EmployeeID = employee.EmployeeID,
            Code = employee.Code,
            Name = employee.Name,
            FName = employee.FName,
            MName = employee.MName,
            ContactNo = employee.ContactNo,
            EmailID = employee.EmailID,
            NID = employee.NID,
            BloodGroup = employee.BloodGroup,
            JoiningDate = employee.JoiningDate,
            PresentAdd = employee.PresentAdd,
            PermanentAdd = employee.PermanentAdd,
            DesignationID = employee.DesignationID,
            DesignationName = employee.Designation?.Description,
            GrossSalary = employee.GrossSalary,
            BasicSalary = employee.BasicSalary,
            PhotoPath = employee.PhotoPath,
            DOB = employee.DOB,
            SRDueLimit = employee.SRDueLimit,
            ConcernID = employee.ConcernID,
            ConcernName = employee.SisterConcern?.Name,
            DepartmentID = employee.DepartmentID,
            DepartmentName = employee.Department?.DESCRIPTION,
            ReligionID = employee.ReligionID,
            ReligionName = employee.Religion?.Name,
            MachineEMPID = employee.MachineEMPID,
            Status = employee.Status
        };
    }
}
