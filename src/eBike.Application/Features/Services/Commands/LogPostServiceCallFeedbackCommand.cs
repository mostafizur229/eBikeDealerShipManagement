using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record LogPostServiceCallFeedbackCommand : IRequest<int>
{
    public int JobCardId { get; set; }
    public int? CustomerId { get; set; }
    public string CallStatus { get; set; } = "Completed";
    public bool IsBikeRunningSmoothly { get; set; } = true;
    public bool IsReportedIssueResolved { get; set; } = true;
    public bool WasDeliveredOnTimeAndClean { get; set; } = true;
    public bool AnyUnofficialChargeDemanded { get; set; } = false;
    public int CsiScore { get; set; } = 10;
    public string? DetailedCustomerVerbatim { get; set; }
    public bool? IsEscalatedToManager { get; set; }
    public string? ManagerEscalationReason { get; set; }
}

public class LogPostServiceCallFeedbackCommandHandler : IRequestHandler<LogPostServiceCallFeedbackCommand, int>
{
    private readonly IApplicationDbContext _context;

    public LogPostServiceCallFeedbackCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(LogPostServiceCallFeedbackCommand request, CancellationToken cancellationToken)
    {
        int custId = request.CustomerId ?? 0;
        if (custId == 0 && request.JobCardId > 0)
        {
            var ticket = await _context.ServiceTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.JobCardId, cancellationToken);
            if (ticket != null)
            {
                custId = ticket.CustomerId;
            }
        }

        // Automatic Royal Enfield SOP Escalation rules:
        // Escalated if CSI <= 4 OR issue not resolved OR bike not running smoothly OR unofficial charge was demanded
        bool shouldEscalate = request.IsEscalatedToManager ??
            (request.CsiScore <= 4 ||
             !request.IsReportedIssueResolved ||
             !request.IsBikeRunningSmoothly ||
             request.AnyUnofficialChargeDemanded);

        string? escalationReason = request.ManagerEscalationReason;
        if (shouldEscalate && string.IsNullOrWhiteSpace(escalationReason))
        {
            if (request.CsiScore <= 4)
                escalationReason = $"Low CSI Score: {request.CsiScore}/10";
            else if (!request.IsReportedIssueResolved)
                escalationReason = "Reported mechanical problem was NOT resolved after service.";
            else if (!request.IsBikeRunningSmoothly)
                escalationReason = "Rider reports motorcycle is not running smoothly.";
            else if (request.AnyUnofficialChargeDemanded)
                escalationReason = "Customer reported unofficial / unauthorized extra charge demanded.";
        }

        var entity = new PostServiceCallFeedback
        {
            JobCardID = request.JobCardId,
            CustomerID = custId,
            CallerEmployeeID = 1,
            CallDateTime = DateTime.UtcNow,
            CallStatus = string.IsNullOrWhiteSpace(request.CallStatus) ? "Completed" : request.CallStatus.Trim(),
            IsBikeRunningSmoothly = request.IsBikeRunningSmoothly,
            IsReportedIssueResolved = request.IsReportedIssueResolved,
            WasDeliveredOnTimeAndClean = request.WasDeliveredOnTimeAndClean,
            AnyUnofficialChargeDemanded = request.AnyUnofficialChargeDemanded,
            CSI_Score = Math.Clamp(request.CsiScore, 1, 10),
            DetailedCustomerVerbatim = request.DetailedCustomerVerbatim?.Trim(),
            IsEscalatedToManager = shouldEscalate,
            ManagerEscalationReason = escalationReason,
            ManagerResolutionStatus = shouldEscalate ? "Open" : "Satisfied",
            ManagerActionNotes = null,
            ConcernID = 3056
        };

        _context.PostServiceCallFeedbacks.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.FeedbackID;
    }
}
