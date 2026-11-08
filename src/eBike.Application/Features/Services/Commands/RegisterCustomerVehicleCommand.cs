using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Exceptions;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record RegisterCustomerVehicleCommand(
    int CustomerId,
    int ProductId,
    string ChassisNo,
    string EngineNo,
    int? ColorId = null,
    string? RegistrationNo = null,
    string? OriginType = "OwnShowroom",
    string? OriginalShowroomName = null,
    string? OriginalInvoiceNo = null,
    DateTime? OriginalPurchaseDate = null,
    decimal CurrentOdometerReading = 0.00m,
    int FreeServicesCompleted = 0,
    int TotalFreeServicesEntitled = 4,
    DateTime? LastServiceDate = null,
    string? CallStatus = null,
    string? CallNotes = null,
    DateTime? AppointmentDate = null
) : IRequest<int>;

public class RegisterCustomerVehicleCommandValidator : AbstractValidator<RegisterCustomerVehicleCommand>
{
    public RegisterCustomerVehicleCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Valid Customer is required.");
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Valid Product / Bike Model is required.");
        RuleFor(x => x.ChassisNo).NotEmpty().WithMessage("Chassis Number is required.");
    }
}

public class RegisterCustomerVehicleCommandHandler : IRequestHandler<RegisterCustomerVehicleCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public RegisterCustomerVehicleCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(RegisterCustomerVehicleCommand request, CancellationToken cancellationToken)
    {
        var chassis = request.ChassisNo.Trim().ToUpper();

        // 1. Check if vehicle already exists by ChassisNo
        var existingVehicle = await _context.CustomerVehicles
            .FirstOrDefaultAsync(v => v.ChassisNo == chassis, cancellationToken);

        if (existingVehicle != null)
        {
            // Update existing vehicle
            existingVehicle.CurrentOdometerReading = request.CurrentOdometerReading > 0 
                ? request.CurrentOdometerReading 
                : existingVehicle.CurrentOdometerReading;
            existingVehicle.FreeServicesCompleted = request.FreeServicesCompleted;
            existingVehicle.TotalFreeServicesEntitled = request.TotalFreeServicesEntitled;
            if (!string.IsNullOrWhiteSpace(request.RegistrationNo))
            {
                existingVehicle.RegistrationNo = request.RegistrationNo.Trim().ToUpper();
            }
            if (request.LastServiceDate.HasValue)
            {
                existingVehicle.LastServiceDate = request.LastServiceDate.Value;
            }
            if (request.ColorId.HasValue && request.ColorId.Value > 0)
            {
                existingVehicle.ColorID = request.ColorId;
            }
            existingVehicle.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return existingVehicle.VehicleID;
        }

        // Determine valid ConcernID
        var customerConcernId = await _context.Customers
            .Where(c => c.CustomerID == request.CustomerId)
            .Select(c => (int?)c.ConcernID)
            .FirstOrDefaultAsync(cancellationToken);

        var validConcernId = _currentUser.ConcernId
            ?? customerConcernId
            ?? await _context.SisterConcerns.Select(s => (int?)s.ConcernID).FirstOrDefaultAsync(cancellationToken)
            ?? 3056;

        // 2. Create new CustomerVehicle
        var vehicle = new CustomerVehicle
        {
            CustomerID = request.CustomerId,
            ProductID = request.ProductId,
            ColorID = request.ColorId > 0 ? request.ColorId : null,
            ChassisNo = chassis,
            EngineNo = string.IsNullOrWhiteSpace(request.EngineNo) ? chassis : request.EngineNo.Trim().ToUpper(),
            RegistrationNo = request.RegistrationNo?.Trim().ToUpper(),
            OriginType = request.OriginType ?? "OwnShowroom",
            OriginalShowroomName = request.OriginalShowroomName?.Trim(),
            OriginalInvoiceNo = request.OriginalInvoiceNo?.Trim(),
            OriginalPurchaseDate = request.OriginalPurchaseDate ?? DateTime.UtcNow.Date,
            CurrentOdometerReading = request.CurrentOdometerReading,
            FreeServicesCompleted = request.FreeServicesCompleted,
            TotalFreeServicesEntitled = request.TotalFreeServicesEntitled,
            LastServiceDate = request.LastServiceDate,
            WarrantyStatus = "Active",
            ConcernID = validConcernId,
            CreatedDate = DateTime.UtcNow
        };

        _context.CustomerVehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

        return vehicle.VehicleID;
    }
}
