using System;

namespace eBike.Application.Features.Services.DTOs;

public class VehiclePeriodicChecklistDto
{
    public int ChecklistId { get; set; }
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
    public DateTime InspectionDate { get; set; }
    public int? InspectorEmployeeId { get; set; }
    public string? InspectorName { get; set; }
    public string OverallStatus { get; set; } = "Compliant";
    public int CompliantCount { get; set; }
    public int ActionRequiredCount { get; set; }
    public string ItemsJson { get; set; } = "[]";
    public string? GeneralNotes { get; set; }
    public DateTime CreatedDate { get; set; }
}
