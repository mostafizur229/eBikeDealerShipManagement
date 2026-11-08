using System;

namespace eBike.Domain.Entities;

public class VehiclePeriodicChecklist
{
    public int ChecklistID { get; set; }
    public int? JobCardID { get; set; }
    public string? TicketNumber { get; set; }
    public int? VehicleID { get; set; }
    public int CustomerID { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string ChassisNo { get; set; } = string.Empty;
    public string? RegistrationNo { get; set; }
    public string BikeModel { get; set; } = string.Empty;
    public decimal OdometerReading { get; set; }
    public string ServiceType { get; set; } = "3rd Free Service";
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public int? InspectorEmployeeID { get; set; }
    public string? InspectorName { get; set; }
    public string OverallStatus { get; set; } = "Compliant"; // Compliant, ActionRequired, InProgress
    public int CompliantCount { get; set; } = 29;
    public int ActionRequiredCount { get; set; } = 0;
    public string ItemsJson { get; set; } = "[]";
    public string? GeneralNotes { get; set; }
    public int ConcernID { get; set; } = 3056;
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
