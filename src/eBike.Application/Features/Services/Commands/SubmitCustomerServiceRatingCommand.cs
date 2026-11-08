using System;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Commands;

public record SubmitCustomerServiceRatingCommand : IRequest<int>
{
    public int JobCardId { get; set; }
    public int? CustomerId { get; set; }
    public int OverallRating { get; set; } = 5;
    public int? PerformanceAndWashingRating { get; set; } = 5;
    public int? AdvisorBehaviorRating { get; set; } = 5;
    public int? BillingTransparencyRating { get; set; } = 5;
    public string? CustomerComments { get; set; }
    public string RatedVia { get; set; } = "Counter Tablet";
}

public class SubmitCustomerServiceRatingCommandHandler : IRequestHandler<SubmitCustomerServiceRatingCommand, int>
{
    private readonly IApplicationDbContext _context;

    public SubmitCustomerServiceRatingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SubmitCustomerServiceRatingCommand request, CancellationToken cancellationToken)
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

        var existing = await _context.CustomerServiceRatings
            .FirstOrDefaultAsync(r => r.JobCardID == request.JobCardId, cancellationToken);

        if (existing != null)
        {
            if (custId > 0) existing.CustomerID = custId;
            existing.OverallRating = Math.Clamp(request.OverallRating, 1, 5);
            existing.PerformanceAndWashingRating = request.PerformanceAndWashingRating.HasValue ? Math.Clamp(request.PerformanceAndWashingRating.Value, 1, 5) : 5;
            existing.AdvisorBehaviorRating = request.AdvisorBehaviorRating.HasValue ? Math.Clamp(request.AdvisorBehaviorRating.Value, 1, 5) : 5;
            existing.BillingTransparencyRating = request.BillingTransparencyRating.HasValue ? Math.Clamp(request.BillingTransparencyRating.Value, 1, 5) : 5;
            existing.CustomerComments = request.CustomerComments?.Trim();
            existing.RatedVia = string.IsNullOrWhiteSpace(request.RatedVia) ? "Counter Tablet" : request.RatedVia.Trim();
            existing.RatedDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return existing.RatingID;
        }

        var entity = new CustomerServiceRating
        {
            JobCardID = request.JobCardId,
            CustomerID = custId,
            OverallRating = Math.Clamp(request.OverallRating, 1, 5),
            PerformanceAndWashingRating = request.PerformanceAndWashingRating.HasValue ? Math.Clamp(request.PerformanceAndWashingRating.Value, 1, 5) : 5,
            AdvisorBehaviorRating = request.AdvisorBehaviorRating.HasValue ? Math.Clamp(request.AdvisorBehaviorRating.Value, 1, 5) : 5,
            BillingTransparencyRating = request.BillingTransparencyRating.HasValue ? Math.Clamp(request.BillingTransparencyRating.Value, 1, 5) : 5,
            CustomerComments = request.CustomerComments?.Trim(),
            RatedVia = string.IsNullOrWhiteSpace(request.RatedVia) ? "Counter Tablet" : request.RatedVia.Trim(),
            RatedDateTime = DateTime.UtcNow
        };

        _context.CustomerServiceRatings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.RatingID;
    }
}
