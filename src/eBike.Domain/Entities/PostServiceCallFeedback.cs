using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eBike.Domain.Entities;

[Table("PostServiceCallFeedbacks")]
public class PostServiceCallFeedback
{
    [Key]
    public int FeedbackID { get; set; }
    public int JobCardID { get; set; }
    public int CustomerID { get; set; }
    public int CallerEmployeeID { get; set; } = 1;
    public DateTime CallDateTime { get; set; } = DateTime.UtcNow;
    public string CallStatus { get; set; } = "Completed"; // Completed, No Answer, Busy, Rescheduled
    public bool IsBikeRunningSmoothly { get; set; } = true;
    public bool IsReportedIssueResolved { get; set; } = true;
    public bool WasDeliveredOnTimeAndClean { get; set; } = true;
    public bool AnyUnofficialChargeDemanded { get; set; } = false;
    public int CSI_Score { get; set; } = 10; // 1 to 10
    public string? DetailedCustomerVerbatim { get; set; }
    public bool IsEscalatedToManager { get; set; } = false;
    public string? ManagerEscalationReason { get; set; }
    public string ManagerResolutionStatus { get; set; } = "Open"; // Open, Under Investigation, Resolved, Closed
    public string? ManagerActionNotes { get; set; }
    public int? ResolvedByManagerEmployeeID { get; set; }
    public DateTime? ResolvedDateTime { get; set; }
    public int ConcernID { get; set; } = 3056;
}
