using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Employees.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Employees.Queries;

public record GetEmployeesQuery(
    string? Search = null,
    int? DesignationId = null,
    int? DepartmentId = null,
    int? Status = null,
    int? ConcernId = null
) : IRequest<List<EmployeeListDto>>;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, List<EmployeeListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetEmployeesQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<EmployeeListDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Designation)
            .Include(e => e.Department)
            .Include(e => e.SisterConcern)
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(e => e.ConcernID == targetConcernId.Value);
        }

        if (request.DesignationId.HasValue && request.DesignationId.Value > 0)
        {
            query = query.Where(e => e.DesignationID == request.DesignationId.Value);
        }

        if (request.DepartmentId.HasValue && request.DepartmentId.Value > 0)
        {
            query = query.Where(e => e.DepartmentID == request.DepartmentId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(e => e.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(e =>
                (e.Name != null && e.Name.ToLower().Contains(search)) ||
                (e.Code != null && e.Code.ToLower().Contains(search)) ||
                (e.ContactNo != null && e.ContactNo.ToLower().Contains(search)) ||
                (e.EmailID != null && e.EmailID.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(e => e.Code)
            .Select(e => new EmployeeListDto
            {
                EmployeeID = e.EmployeeID,
                Code = e.Code,
                Name = e.Name,
                ContactNo = e.ContactNo,
                EmailID = e.EmailID,
                JoiningDate = e.JoiningDate,
                DesignationID = e.DesignationID,
                DesignationName = e.Designation != null ? e.Designation.Description : null,
                DepartmentID = e.DepartmentID,
                DepartmentName = e.Department != null ? e.Department.DESCRIPTION : null,
                GrossSalary = e.GrossSalary,
                MachineEMPID = e.MachineEMPID,
                Status = e.Status,
                ConcernID = e.ConcernID,
                ConcernName = e.SisterConcern != null ? e.SisterConcern.Name : null
            })
            .ToListAsync(cancellationToken);
    }
}
