using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Departments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Departments.Queries;

public record GetDepartmentsQuery(string? Search = null) : IRequest<List<DepartmentListDto>>;

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, List<DepartmentListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentListDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Departments
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(d =>
                (d.DESCRIPTION != null && d.DESCRIPTION.ToLower().Contains(search)) ||
                (d.CODE != null && d.CODE.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(d => d.CODE)
            .Select(d => new DepartmentListDto
            {
                DepartmentId = d.DepartmentId,
                Code = d.CODE,
                Description = d.DESCRIPTION,
                Status = d.Status
            })
            .ToListAsync(cancellationToken);
    }
}
