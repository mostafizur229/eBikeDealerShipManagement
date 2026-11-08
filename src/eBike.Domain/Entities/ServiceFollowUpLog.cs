using System;

namespace eBike.Domain.Entities;

public class ServiceFollowUpLog
{
    public int FollowUpLogID { get; set; }
    public int? VehicleID { get; set; }
    public CustomerVehicle? Vehicle { get; set; }

    public int CustomerID { get; set; }
    public Customer Customer { get; set; } = null!;

    public string? ChassisNo { get; set; }
    public string CallStatus { get; set; } = "Pending";
    public string? CallNotes { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public DateTime? AppointmentDate { get; set; }

    public int? CallerEmployeeID { get; set; }
    public string? CallerName { get; set; }
    public DateTime CallDateTime { get; set; } = DateTime.UtcNow;

    public int ConcernID { get; set; } = 3056;
    public SisterConcern? SisterConcern { get; set; }
}
