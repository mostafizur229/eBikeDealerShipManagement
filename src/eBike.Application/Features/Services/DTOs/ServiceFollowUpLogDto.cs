using System;

namespace eBike.Application.Features.Services.DTOs;

public class ServiceFollowUpLogDto
{
    public int FollowUpLogId { get; set; }
    public int? VehicleId { get; set; }
    public int CustomerId { get; set; }
    public string? ChassisNo { get; set; }
    public string CallStatus { get; set; } = string.Empty;
    public string? CallNotes { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? CallerName { get; set; }
    public DateTime CallDateTime { get; set; }
}
