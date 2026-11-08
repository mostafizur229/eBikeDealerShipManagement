using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record AdvanceVehicleMilestoneCommand(
    string ChassisNo,
    int? CustomerId = null,
    int? CurrentMileageKm = null,
    DateTime? CompletionDate = null,
    string? Notes = null
) : IRequest<bool>;

public class AdvanceVehicleMilestoneCommandHandler : IRequestHandler<AdvanceVehicleMilestoneCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AdvanceVehicleMilestoneCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(AdvanceVehicleMilestoneCommand request, CancellationToken cancellationToken)
    {
        var chassis = request.ChassisNo?.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(chassis)) return false;

        var vehicle = await _context.CustomerVehicles
            .FirstOrDefaultAsync(v => v.ChassisNo == chassis, cancellationToken);

        var completionDate = request.CompletionDate ?? DateTime.UtcNow;

        if (vehicle != null)
        {
            vehicle.FreeServicesCompleted += 1;
            vehicle.LastServiceDate = completionDate;
            if (request.CurrentMileageKm.HasValue && request.CurrentMileageKm.Value > 0)
            {
                vehicle.CurrentOdometerReading = request.CurrentMileageKm.Value;
            }
            vehicle.RelocationReasonNotes = $"[ServiceCompleted: {completionDate:yyyy-MM-dd}] Milestone #{vehicle.FreeServicesCompleted} finished. Next milestone due.";
            vehicle.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            // If vehicle not in CustomerVehicles, check SOrders to create it
            var sorder = await _context.SOrderDetails
                .Include(d => d.SOrder)
                .Where(d => d.StockDetail != null && d.StockDetail.IMENO == chassis)
                .Select(d => new
                {
                    CustomerID = (int?)d.SOrder.CustomerID,
                    ProductID = (int?)d.ProductID,
                    InvoiceDate = (DateTime?)d.SOrder.InvoiceDate
                })
                .FirstOrDefaultAsync(cancellationToken);

            var concernId = _currentUser.ConcernId ?? 3056;

            var newVehicle = new CustomerVehicle
            {
                CustomerID = request.CustomerId ?? sorder?.CustomerID ?? 1,
                ProductID = sorder?.ProductID ?? 1,
                ChassisNo = chassis,
                EngineNo = chassis,
                OriginType = "OwnShowroom",
                OriginalPurchaseDate = sorder?.InvoiceDate ?? completionDate.AddDays(-45),
                CurrentOdometerReading = request.CurrentMileageKm ?? 1000,
                FreeServicesCompleted = 1,
                TotalFreeServicesEntitled = 4,
                LastServiceDate = completionDate,
                WarrantyStatus = "Active",
                RelocationReasonNotes = $"[ServiceCompleted: {completionDate:yyyy-MM-dd}] 1st Service finished.",
                ConcernID = concernId,
                CreatedDate = DateTime.UtcNow
            };
            _context.CustomerVehicles.Add(newVehicle);
        }

        // Add to call follow-up log as service completed record
        _context.ServiceFollowUpLogs.Add(new ServiceFollowUpLog
        {
            ChassisNo = chassis,
            CustomerID = vehicle?.CustomerID ?? request.CustomerId ?? 1,
            VehicleID = vehicle?.VehicleID,
            CallDateTime = completionDate,
            CallStatus = "ServiceCompleted",
            CallNotes = $"Service milestone completed ({completionDate:yyyy-MM-dd}). Moved to next milestone. {request.Notes}",
            CallerEmployeeID = 1
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
