using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Customers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Customers.Queries;

public record GetCustomersQuery(
    string? Search = null,
    int? CustomerType = null,
    int? DistrictId = null,
    int? ThanaId = null,
    int? EmployeeId = null,
    int? ConcernId = null
) : IRequest<List<CustomerListDto>>;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetCustomersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CustomerListDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        int? targetConcernId = request.ConcernId ?? _currentUser.ConcernId;

        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.District)
            .Include(c => c.ThanaObj)
            .Include(c => c.Employee)
            .AsQueryable();

        if (targetConcernId.HasValue)
        {
            query = query.Where(c => c.ConcernID == targetConcernId.Value);
        }

        if (request.CustomerType.HasValue && request.CustomerType > 0)
        {
            query = query.Where(c => c.CustomerType == request.CustomerType.Value);
        }

        if (request.DistrictId.HasValue && request.DistrictId > 0)
        {
            query = query.Where(c => c.DistrictID == request.DistrictId.Value);
        }

        if (request.ThanaId.HasValue && request.ThanaId > 0)
        {
            query = query.Where(c => c.ThanaID == request.ThanaId.Value);
        }

        if (request.EmployeeId.HasValue && request.EmployeeId > 0)
        {
            query = query.Where(c => c.EmployeeID == request.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(search)) ||
                (c.Code != null && c.Code.ToLower().Contains(search)) ||
                (c.ContactNo != null && c.ContactNo.ToLower().Contains(search)) ||
                (c.NID != null && c.NID.ToLower().Contains(search)) ||
                (c.EmailID != null && c.EmailID.ToLower().Contains(search)));
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CustomerListDto
            {
                CustomerID = c.CustomerID,
                Code = c.Code,
                Name = c.Name,
                ContactNo = c.ContactNo,
                Address = c.Address,
                CustomerType = c.CustomerType,
                CustomerTypeName = c.CustomerType == 1 ? "Retail" :
                                   c.CustomerType == 2 ? "Dealer" :
                                   c.CustomerType == 3 ? "Credit" : "Branch",
                TotalDue = c.TotalDue,
                OpeningDue = c.OpeningDue,
                DistrictID = c.DistrictID,
                DistrictName = c.District != null ? c.District.Name : null,
                ThanaID = c.ThanaID,
                ThanaName = c.ThanaObj != null ? c.ThanaObj.Name : null,
                EmployeeID = c.EmployeeID,
                EmployeeName = c.Employee != null ? c.Employee.Name : null,
                ConcernID = c.ConcernID
            })
            .ToListAsync(cancellationToken);
    }
}
