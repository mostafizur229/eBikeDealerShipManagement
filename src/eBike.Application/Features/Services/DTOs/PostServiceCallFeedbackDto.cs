using System;

namespace eBike.Application.Features.Services.DTOs;

public class PostServiceCallFeedbackDto
{
    public int FeedbackId { get; set; }
    public int JobCardId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string BikeModel { get; set; } = string.Empty;
    public DateTime? ServiceDate { get; set; }
    public int CallerEmployeeId { get; set; }
    public DateTime CallDateTime { get; set; }
    public string CallStatus { get; set; } = "Completed";
    public bool IsBikeRunningSmoothly { get; set; }
    public bool IsReportedIssueResolved { get; set; }
    public bool WasDeliveredOnTimeAndClean { get; set; }
    public bool AnyUnofficialChargeDemanded { get; set; }
    public int CsiScore { get; set; }
    public string? DetailedCustomerVerbatim { get; set; }
    public bool IsEscalatedToManager { get; set; }
    public string? ManagerEscalationReason { get; set; }
    public string ManagerResolutionStatus { get; set; } = "Open";
    public string? ManagerActionNotes { get; set; }
    public DateTime? ResolvedDateTime { get; set; }
}
