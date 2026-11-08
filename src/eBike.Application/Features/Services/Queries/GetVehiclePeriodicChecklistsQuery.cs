using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetVehiclePeriodicChecklistsQuery(
    string? ChassisNo = null,
    int? CustomerId = null,
    int? JobCardId = null,
    string? Search = null
) : IRequest<List<VehiclePeriodicChecklistDto>>;

public class GetVehiclePeriodicChecklistsQueryHandler : IRequestHandler<GetVehiclePeriodicChecklistsQuery, List<VehiclePeriodicChecklistDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVehiclePeriodicChecklistsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehiclePeriodicChecklistDto>> Handle(GetVehiclePeriodicChecklistsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.VehiclePeriodicChecklists.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ChassisNo))
        {
            var cleanChassis = request.ChassisNo.Trim();
            query = query.Where(c => c.ChassisNo == cleanChassis);
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            query = query.Where(c => c.CustomerID == request.CustomerId.Value);
        }

        if (request.JobCardId.HasValue && request.JobCardId.Value > 0)
        {
            query = query.Where(c => c.JobCardID == request.JobCardId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(c =>
                c.CustomerName.ToLower().Contains(s) ||
                (c.CustomerPhone != null && c.CustomerPhone.Contains(s)) ||
                c.ChassisNo.ToLower().Contains(s) ||
                (c.RegistrationNo != null && c.RegistrationNo.ToLower().Contains(s)) ||
                c.BikeModel.ToLower().Contains(s) ||
                (c.TicketNumber != null && c.TicketNumber.ToLower().Contains(s)));
        }

        return await query
            .OrderByDescending(c => c.InspectionDate)
            .Take(100)
            .Select(c => new VehiclePeriodicChecklistDto
            {
                ChecklistId = c.ChecklistID,
                JobCardId = c.JobCardID,
                TicketNumber = c.TicketNumber,
                VehicleId = c.VehicleID,
                CustomerId = c.CustomerID,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                ChassisNo = c.ChassisNo,
                RegistrationNo = c.RegistrationNo,
                BikeModel = c.BikeModel,
                OdometerReading = c.OdometerReading,
                ServiceType = c.ServiceType,
                InspectionDate = c.InspectionDate,
                InspectorEmployeeId = c.InspectorEmployeeID,
                InspectorName = c.InspectorName,
                OverallStatus = c.OverallStatus,
                CompliantCount = c.CompliantCount,
                ActionRequiredCount = c.ActionRequiredCount,
                ItemsJson = c.ItemsJson,
                GeneralNotes = c.GeneralNotes,
                CreatedDate = c.CreatedDate
            })
            .ToListAsync(cancellationToken);
    }
}
