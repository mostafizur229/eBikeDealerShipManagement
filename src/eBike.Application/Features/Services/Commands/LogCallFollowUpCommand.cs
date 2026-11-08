using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record LogCallFollowUpCommand(
    int VehicleId,
    int CustomerId,
    string? ChassisNo,
    int? ProductId,
    string CallStatus,
    string? CallNotes,
    DateTime? NextFollowUpDate,
    DateTime? AppointmentDate
) : IRequest<bool>;

public class LogCallFollowUpCommandHandler : IRequestHandler<LogCallFollowUpCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public LogCallFollowUpCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(LogCallFollowUpCommand request, CancellationToken cancellationToken)
    {
        var noteSummary = $"[{request.CallStatus}] " +
            (request.NextFollowUpDate.HasValue ? $"Next Follow-Up: {request.NextFollowUpDate.Value:yyyy-MM-dd}. " : "") +
            (request.AppointmentDate.HasValue ? $"Appt: {request.AppointmentDate.Value:yyyy-MM-dd HH:mm}. " : "") +
            (request.CallNotes ?? "");

        var customerConcernId = await _context.Customers
            .Where(c => c.CustomerID == request.CustomerId)
            .Select(c => (int?)c.ConcernID)
            .FirstOrDefaultAsync(cancellationToken);

        var validConcernId = _currentUser.ConcernId
            ?? customerConcernId
            ?? await _context.SisterConcerns.Select(s => (int?)s.ConcernID).FirstOrDefaultAsync(cancellationToken)
            ?? 3056;

        CustomerVehicle? vehicle = null;

        if (request.VehicleId > 0 && request.VehicleId < 100000)
        {
            vehicle = await _context.CustomerVehicles
                .FirstOrDefaultAsync(v => v.VehicleID == request.VehicleId, cancellationToken);
        }

        if (vehicle == null && !string.IsNullOrWhiteSpace(request.ChassisNo))
        {
            var chassis = request.ChassisNo.Trim().ToUpper();
            vehicle = await _context.CustomerVehicles
                .FirstOrDefaultAsync(v => v.ChassisNo == chassis, cancellationToken);
        }

        if (vehicle != null)
        {
            vehicle.RelocationReasonNotes = noteSummary;
            vehicle.ModifiedDate = DateTime.UtcNow;
            if (request.AppointmentDate.HasValue)
            {
                vehicle.LastServiceDate = request.AppointmentDate.Value.Date;
            }
        }
        else if (request.CustomerId > 0 && !string.IsNullOrWhiteSpace(request.ChassisNo))
        {
            // Auto-enroll vehicle into CustomerVehicles with follow-up tracking
            vehicle = new CustomerVehicle
            {
                CustomerID = request.CustomerId,
                ProductID = request.ProductId ?? 47239,
                ChassisNo = request.ChassisNo.Trim().ToUpper(),
                EngineNo = request.ChassisNo.Trim().ToUpper(),
                OriginType = "OwnShowroom",
                RelocationReasonNotes = noteSummary,
                WarrantyStatus = "Active",
                TotalFreeServicesEntitled = 4,
                FreeServicesCompleted = 0,
                ConcernID = validConcernId,
                CreatedDate = DateTime.UtcNow
            };
            _context.CustomerVehicles.Add(vehicle);
        }

        // Always add a new record to the full call history log table
        var followUpLog = new ServiceFollowUpLog
        {
            Vehicle = vehicle,
            CustomerID = request.CustomerId,
            ChassisNo = request.ChassisNo?.Trim().ToUpper(),
            CallStatus = request.CallStatus,
            CallNotes = request.CallNotes,
            NextFollowUpDate = request.NextFollowUpDate,
            AppointmentDate = request.AppointmentDate,
            CallerEmployeeID = _currentUser.EmployeeId,
            CallerName = !string.IsNullOrWhiteSpace(_currentUser.Username) ? _currentUser.Username : "Staff",
            CallDateTime = DateTime.UtcNow,
            ConcernID = validConcernId
        };
        _context.ServiceFollowUpLogs.Add(followUpLog);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
