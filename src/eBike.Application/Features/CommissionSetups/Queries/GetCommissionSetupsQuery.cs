using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.CommissionSetups.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.CommissionSetups.Queries;

public record GetCommissionSetupsQuery(
    string? Search = null,
    int? EmployeeId = null,
    int? Year = null,
    int? Month = null
) : IRequest<List<CommissionSetupListDto>>;

public class GetCommissionSetupsQueryHandler : IRequestHandler<GetCommissionSetupsQuery, List<CommissionSetupListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCommissionSetupsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommissionSetupListDto>> Handle(GetCommissionSetupsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CommissionSetups
            .Include(c => c.Employee)
            .AsNoTracking();

        if (request.EmployeeId.HasValue && request.EmployeeId > 0)
        {
            query = query.Where(c => c.EmployeeID == request.EmployeeId.Value);
        }

        if (request.Year.HasValue && request.Month.HasValue)
        {
            var firstDay = new DateTime(request.Year.Value, request.Month.Value, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            query = query.Where(c => c.CommissionMonth >= firstDay && c.CommissionMonth <= lastDay);
        }
        else if (request.Year.HasValue)
        {
            var firstDay = new DateTime(request.Year.Value, 1, 1);
            var lastDay = new DateTime(request.Year.Value, 12, 31);
            query = query.Where(c => c.CommissionMonth >= firstDay && c.CommissionMonth <= lastDay);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(c => c.Employee != null && (c.Employee.Name.ToLower().Contains(search) || c.Employee.Code.ToLower().Contains(search)));
        }

        var items = await query.OrderByDescending(c => c.CommissionMonth).ThenBy(c => c.CSID).ToListAsync(cancellationToken);

        return items.Select(c => new CommissionSetupListDto
        {
            CSID = c.CSID,
            EmployeeID = c.EmployeeID,
            EmployeeName = c.Employee?.Name,
            EmployeeCode = c.Employee?.Code,
            AchievedPercentStart = c.AchievedPercentStart,
            AchievedPercentEnd = c.AchievedPercentEnd,
            CommissionMonth = c.CommissionMonth,
            CommisssionAmt = c.CommisssionAmt,
            CommissionPercent = c.CommissionPercent,
            Status = c.Status
        }).ToList();
    }
}

public record GetCommissionSetupByIdQuery(int Id) : IRequest<CommissionSetupDto>;

public class GetCommissionSetupByIdQueryHandler : IRequestHandler<GetCommissionSetupByIdQuery, CommissionSetupDto>
{
    private readonly IApplicationDbContext _context;

    public GetCommissionSetupByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommissionSetupDto> Handle(GetCommissionSetupByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.CommissionSetups
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.CSID == request.Id, cancellationToken);

        if (c == null)
        {
            throw new NotFoundException("CommissionSetup", request.Id.ToString());
        }

        return new CommissionSetupDto
        {
            CSID = c.CSID,
            EmployeeID = c.EmployeeID,
            EmployeeName = c.Employee?.Name,
            EmployeeCode = c.Employee?.Code,
            AchievedPercentStart = c.AchievedPercentStart,
            AchievedPercentEnd = c.AchievedPercentEnd,
            CommissionMonth = c.CommissionMonth,
            CommisssionAmt = c.CommisssionAmt,
            CommissionPercent = c.CommissionPercent,
            Status = c.Status,
            ConcernID = c.ConcernID
        };
    }
}

public record GetDesWiseCommissionsQuery() : IRequest<List<DesWiseCommissionDto>>;

public class GetDesWiseCommissionsQueryHandler : IRequestHandler<GetDesWiseCommissionsQuery, List<DesWiseCommissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDesWiseCommissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DesWiseCommissionDto>> Handle(GetDesWiseCommissionsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.DesWiseCommissions
            .Include(d => d.Designation)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return items.Select(d => new DesWiseCommissionDto
        {
            ID = d.ID,
            CommissionPercent = d.CommissionPercent,
            DesignationID = d.DesignationID,
            DesignationName = d.Designation?.Description,
            DesignationCode = d.Designation?.Code,
            ConcernID = d.ConcernID
        }).ToList();
    }
}
