using System;

namespace eBike.Application.Features.Services.DTOs;

public class ServiceDueDto
{
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string BikeModel { get; set; } = string.Empty;
    public string ChassisNumber { get; set; } = string.Empty;
    public string? RegistrationNo { get; set; }
    public DateTime SalesDate { get; set; }
    public int CurrentMileageKm { get; set; }
    public int FreeServicesCompleted { get; set; }
    public int TotalFreeServicesEntitled { get; set; } = 4;
    public string NextMilestoneName { get; set; } = string.Empty; // e.g. "1st Free Service (500 KM / 45 Days)"
    public int TargetMileageKm { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdueOrRemaining { get; set; }
    public string DueStatus { get; set; } = "Upcoming"; // "Overdue", "DueThisWeek", "Upcoming"
    public string CallStatus { get; set; } = "Pending"; // "Pending", "CallAnswered", "AppointmentBooked", "CallLater", "NoAnswer"
    public string? CallNotes { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public int TotalCallAttempts { get; set; } = 0;
}
