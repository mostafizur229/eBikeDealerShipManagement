using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetCustomerCallHistoryQuery(int? VehicleId = null, int? CustomerId = null, string? ChassisNo = null) : IRequest<List<ServiceFollowUpLogDto>>;

public class GetCustomerCallHistoryQueryHandler : IRequestHandler<GetCustomerCallHistoryQuery, List<ServiceFollowUpLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerCallHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceFollowUpLogDto>> Handle(GetCustomerCallHistoryQuery request, CancellationToken cancellationToken)
    {
        var chassis = request.ChassisNo?.Trim().ToUpper();

        var query = _context.ServiceFollowUpLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(chassis))
        {
            query = query.Where(l => l.ChassisNo == chassis || (request.CustomerId.HasValue && l.CustomerID == request.CustomerId.Value));
        }
        else if (request.VehicleId.HasValue && request.VehicleId.Value > 0 && request.VehicleId.Value < 100000)
        {
            query = query.Where(l => l.VehicleID == request.VehicleId.Value);
        }
        else if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            query = query.Where(l => l.CustomerID == request.CustomerId.Value);
        }
        else
        {
            return new List<ServiceFollowUpLogDto>();
        }

        var logs = await query
            .OrderByDescending(l => l.CallDateTime)
            .Take(50)
            .Select(l => new ServiceFollowUpLogDto
            {
                FollowUpLogId = l.FollowUpLogID,
                VehicleId = l.VehicleID,
                CustomerId = l.CustomerID,
                ChassisNo = l.ChassisNo,
                CallStatus = l.CallStatus,
                CallNotes = l.CallNotes,
                NextFollowUpDate = l.NextFollowUpDate,
                AppointmentDate = l.AppointmentDate,
                CallerName = l.CallerName,
                CallDateTime = l.CallDateTime
            })
            .ToListAsync(cancellationToken);

        return logs;
    }
}
