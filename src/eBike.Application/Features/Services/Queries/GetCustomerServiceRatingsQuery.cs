using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eBike.Application.Common.Interfaces;
using eBike.Application.Features.Services.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Features.Services.Queries;

public record GetCustomerServiceRatingsQuery(string? Search = null, int? MinRating = null, int Limit = 100) : IRequest<List<CustomerServiceRatingDto>>;

public class GetCustomerServiceRatingsQueryHandler : IRequestHandler<GetCustomerServiceRatingsQuery, List<CustomerServiceRatingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerServiceRatingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerServiceRatingDto>> Handle(GetCustomerServiceRatingsQuery request, CancellationToken cancellationToken)
    {
        var ratingsQuery = _context.CustomerServiceRatings
            .AsNoTracking()
            .OrderByDescending(r => r.RatedDateTime)
            .AsQueryable();

        if (request.MinRating.HasValue)
        {
            ratingsQuery = ratingsQuery.Where(r => r.OverallRating >= request.MinRating.Value);
        }

        var rawList = await ratingsQuery.Take(request.Limit).ToListAsync(cancellationToken);

        if (rawList.Count == 0)
        {
            return new List<CustomerServiceRatingDto>();
        }

        // Fetch related Service Tickets with Customer and EBike
        var jobCardIds = rawList.Select(r => r.JobCardID).Distinct().ToList();
        var tickets = await _context.ServiceTickets
            .Include(t => t.Customer)
            .Include(t => t.EBike)
            .AsNoTracking()
            .Where(t => jobCardIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t, cancellationToken);

        var custIds = rawList.Select(r => r.CustomerID).Distinct().ToList();
        var customers = await _context.Customers
            .AsNoTracking()
            .Where(c => custIds.Contains(c.CustomerID))
            .ToDictionaryAsync(c => c.CustomerID, c => c, cancellationToken);

        var search = request.Search?.Trim().ToLower();

        var result = new List<CustomerServiceRatingDto>();

        foreach (var r in rawList)
        {
            tickets.TryGetValue(r.JobCardID, out var ticket);
            customers.TryGetValue(r.CustomerID, out var customer);

            var ticketNumber = ticket?.TicketNumber ?? $"JC-RE-2026-{r.JobCardID:D4}";
            var customerName = customer?.Name ?? ticket?.Customer?.Name ?? "Customer";
            var customerPhone = customer?.ContactNo ?? ticket?.Customer?.ContactNo ?? "";
            var bikeModel = ticket?.EBike?.Model ?? "Classic 350";

            if (!string.IsNullOrWhiteSpace(search))
            {
                bool matches = ticketNumber.ToLower().Contains(search) ||
                               customerName.ToLower().Contains(search) ||
                               customerPhone.Contains(search) ||
                               bikeModel.ToLower().Contains(search) ||
                               (r.CustomerComments != null && r.CustomerComments.ToLower().Contains(search)) ||
                               r.RatedVia.ToLower().Contains(search);

                if (!matches) continue;
            }

            result.Add(new CustomerServiceRatingDto
            {
                RatingId = r.RatingID,
                JobCardId = r.JobCardID,
                TicketNumber = ticketNumber,
                CustomerId = r.CustomerID,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                BikeModel = bikeModel,
                OverallRating = r.OverallRating,
                PerformanceAndWashingRating = r.PerformanceAndWashingRating ?? r.OverallRating,
                AdvisorBehaviorRating = r.AdvisorBehaviorRating ?? r.OverallRating,
                BillingTransparencyRating = r.BillingTransparencyRating ?? r.OverallRating,
                CustomerComments = r.CustomerComments,
                RatedVia = r.RatedVia,
                RatedDateTime = r.RatedDateTime
            });
        }

        return result;
    }
}
