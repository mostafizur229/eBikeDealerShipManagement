using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Departments.DTOs;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Departments.Queries;

public record GetDepartmentByIdQuery(int Id) : IRequest<DepartmentDto>;

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentId == request.Id, cancellationToken);

        if (department == null)
        {
            throw new NotFoundException(nameof(Department), request.Id);
        }

        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            Code = department.CODE,
            Description = department.DESCRIPTION,
            Status = department.Status
        };
    }
}
