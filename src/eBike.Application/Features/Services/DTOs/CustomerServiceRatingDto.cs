using System;

namespace eBike.Application.Features.Services.DTOs;

public class CustomerServiceRatingDto
{
    public int RatingId { get; set; }
    public int JobCardId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string BikeModel { get; set; } = string.Empty;
    public int OverallRating { get; set; }
    public int PerformanceAndWashingRating { get; set; }
    public int AdvisorBehaviorRating { get; set; }
    public int BillingTransparencyRating { get; set; }
    public string? CustomerComments { get; set; }
    public string RatedVia { get; set; } = "Counter Tablet";
    public DateTime RatedDateTime { get; set; }
}
