using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public class SaveVehiclePeriodicChecklistCommand : IRequest<int>
{
    public int? ChecklistId { get; set; }
    public int? JobCardId { get; set; }
    public string? TicketNumber { get; set; }
    public int? VehicleId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ChassisNo { get; set; } = string.Empty;
    public string? RegistrationNo { get; set; }
    public string BikeModel { get; set; } = string.Empty;
    public decimal OdometerReading { get; set; }
    public string ServiceType { get; set; } = "3rd Free Service";
    public int? InspectorEmployeeId { get; set; }
    public string? InspectorName { get; set; }
    public string OverallStatus { get; set; } = "Compliant";
    public int CompliantCount { get; set; } = 29;
    public int ActionRequiredCount { get; set; } = 0;
    public string ItemsJson { get; set; } = "[]";
    public string? GeneralNotes { get; set; }
}

public class SaveVehiclePeriodicChecklistCommandHandler : IRequestHandler<SaveVehiclePeriodicChecklistCommand, int>
{
    private readonly IApplicationDbContext _context;

    public SaveVehiclePeriodicChecklistCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SaveVehiclePeriodicChecklistCommand request, CancellationToken cancellationToken)
    {
        VehiclePeriodicChecklist? entity = null;

        if (request.ChecklistId.HasValue && request.ChecklistId.Value > 0)
        {
            entity = await _context.VehiclePeriodicChecklists
                .FirstOrDefaultAsync(c => c.ChecklistID == request.ChecklistId.Value, cancellationToken);
        }

        if (entity == null && !string.IsNullOrWhiteSpace(request.ChassisNo) && request.JobCardId.HasValue && request.JobCardId.Value > 0)
        {
            entity = await _context.VehiclePeriodicChecklists
                .FirstOrDefaultAsync(c => c.JobCardID == request.JobCardId.Value, cancellationToken);
        }

        if (entity == null)
        {
            entity = new VehiclePeriodicChecklist
            {
                JobCardID = request.JobCardId,
                TicketNumber = request.TicketNumber,
                VehicleID = request.VehicleId,
                CustomerID = request.CustomerId,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                ChassisNo = request.ChassisNo,
                RegistrationNo = request.RegistrationNo,
                BikeModel = request.BikeModel,
                OdometerReading = request.OdometerReading,
                ServiceType = request.ServiceType,
                InspectionDate = DateTime.UtcNow,
                InspectorEmployeeID = request.InspectorEmployeeId,
                InspectorName = request.InspectorName,
                OverallStatus = request.OverallStatus,
                CompliantCount = request.CompliantCount,
                ActionRequiredCount = request.ActionRequiredCount,
                ItemsJson = request.ItemsJson,
                GeneralNotes = request.GeneralNotes,
                CreatedDate = DateTime.UtcNow
            };

            _context.VehiclePeriodicChecklists.Add(entity);
        }
        else
        {
            entity.JobCardID = request.JobCardId ?? entity.JobCardID;
            entity.TicketNumber = request.TicketNumber ?? entity.TicketNumber;
            entity.VehicleID = request.VehicleId ?? entity.VehicleID;
            entity.CustomerID = request.CustomerId > 0 ? request.CustomerId : entity.CustomerID;
            entity.CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? entity.CustomerName : request.CustomerName;
            entity.CustomerPhone = request.CustomerPhone ?? entity.CustomerPhone;
            entity.ChassisNo = string.IsNullOrWhiteSpace(request.ChassisNo) ? entity.ChassisNo : request.ChassisNo;
            entity.RegistrationNo = request.RegistrationNo ?? entity.RegistrationNo;
            entity.BikeModel = string.IsNullOrWhiteSpace(request.BikeModel) ? entity.BikeModel : request.BikeModel;
            entity.OdometerReading = request.OdometerReading;
            entity.ServiceType = request.ServiceType;
            entity.InspectorEmployeeID = request.InspectorEmployeeId ?? entity.InspectorEmployeeID;
            entity.InspectorName = request.InspectorName ?? entity.InspectorName;
            entity.OverallStatus = request.OverallStatus;
            entity.CompliantCount = request.CompliantCount;
            entity.ActionRequiredCount = request.ActionRequiredCount;
            entity.ItemsJson = request.ItemsJson;
            entity.GeneralNotes = request.GeneralNotes;
            entity.ModifiedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return entity.ChecklistID;
    }
}
