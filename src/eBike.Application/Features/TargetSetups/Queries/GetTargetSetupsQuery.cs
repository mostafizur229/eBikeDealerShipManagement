using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.TargetSetups.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.TargetSetups.Queries;

public record GetTargetSetupsQuery(
    string? Search = null,
    int? EmployeeId = null,
    int? Year = null,
    int? Month = null
) : IRequest<List<TargetSetupListDto>>;

public class GetTargetSetupsQueryHandler : IRequestHandler<GetTargetSetupsQuery, List<TargetSetupListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTargetSetupsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TargetSetupListDto>> Handle(GetTargetSetupsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TargetSetups
            .Include(t => t.Employee)
            .Include(t => t.TargetSetupDetails)
            .AsNoTracking();

        if (request.EmployeeId.HasValue && request.EmployeeId > 0)
        {
            query = query.Where(t => t.EmployeeID == request.EmployeeId.Value);
        }

        if (request.Year.HasValue && request.Month.HasValue)
        {
            var firstDay = new DateTime(request.Year.Value, request.Month.Value, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            query = query.Where(t => t.TargetMonth >= firstDay && t.TargetMonth <= lastDay);
        }
        else if (request.Year.HasValue)
        {
            var firstDay = new DateTime(request.Year.Value, 1, 1);
            var lastDay = new DateTime(request.Year.Value, 12, 31);
            query = query.Where(t => t.TargetMonth >= firstDay && t.TargetMonth <= lastDay);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(t => t.Employee != null && (t.Employee.Name.ToLower().Contains(search) || t.Employee.Code.ToLower().Contains(search)));
        }

        var items = await query.OrderByDescending(t => t.TargetMonth).ThenBy(t => t.TID).ToListAsync(cancellationToken);

        return items.Select(t => new TargetSetupListDto
        {
            TID = t.TID,
            EmployeeID = t.EmployeeID,
            EmployeeName = t.Employee?.Name,
            EmployeeCode = t.Employee?.Code,
            TargetMonth = t.TargetMonth,
            Quantity = t.Quantity,
            Amount = t.Amount,
            Status = t.Status,
            DetailCount = t.TargetSetupDetails.Count
        }).ToList();
    }
}

public record GetTargetSetupByIdQuery(int Id) : IRequest<TargetSetupDto>;

public class GetTargetSetupByIdQueryHandler : IRequestHandler<GetTargetSetupByIdQuery, TargetSetupDto>
{
    private readonly IApplicationDbContext _context;

    public GetTargetSetupByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TargetSetupDto> Handle(GetTargetSetupByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _context.TargetSetups
            .Include(x => x.Employee)
            .Include(x => x.TargetSetupDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.TID == request.Id, cancellationToken);

        if (t == null)
        {
            throw new NotFoundException("TargetSetup", request.Id.ToString());
        }

        return new TargetSetupDto
        {
            TID = t.TID,
            EmployeeID = t.EmployeeID,
            EmployeeName = t.Employee?.Name,
            EmployeeCode = t.Employee?.Code,
            TargetMonth = t.TargetMonth,
            Quantity = t.Quantity,
            Amount = t.Amount,
            Status = t.Status,
            ConcernID = t.ConcernID,
            Details = t.TargetSetupDetails.Select(d => new TargetSetupDetailDto
            {
                ID = d.ID,
                ProductID = d.ProductID,
                ProductName = d.Product?.ProductName,
                TID = d.TID,
                Quantity = d.Quantity
            }).ToList()
        };
    }
}
