using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record ResolveManagerEscalationCommand : IRequest<bool>
{
    public int FeedbackId { get; set; }
    public string ResolutionStatus { get; set; } = "Resolved"; // Under Investigation, Resolved, Closed
    public string? ManagerActionNotes { get; set; }
}

public class ResolveManagerEscalationCommandHandler : IRequestHandler<ResolveManagerEscalationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ResolveManagerEscalationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ResolveManagerEscalationCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _context.PostServiceCallFeedbacks
            .FirstOrDefaultAsync(f => f.FeedbackID == request.FeedbackId, cancellationToken);

        if (feedback == null) return false;

        feedback.ManagerResolutionStatus = string.IsNullOrWhiteSpace(request.ResolutionStatus) ? "Resolved" : request.ResolutionStatus.Trim();
        if (!string.IsNullOrWhiteSpace(request.ManagerActionNotes))
        {
            feedback.ManagerActionNotes = request.ManagerActionNotes.Trim();
        }

        if (feedback.ManagerResolutionStatus == "Resolved" || feedback.ManagerResolutionStatus == "Closed")
        {
            feedback.ResolvedDateTime = DateTime.UtcNow;
            feedback.ResolvedByManagerEmployeeID = 1;
            feedback.IsEscalatedToManager = false;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
