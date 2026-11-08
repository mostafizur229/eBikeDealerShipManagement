using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eBike.Domain.Entities;

[Table("CustomerServiceRatings")]
public class CustomerServiceRating
{
    [Key]
    public int RatingID { get; set; }
    public int JobCardID { get; set; }
    public int CustomerID { get; set; }
    public int OverallRating { get; set; } = 5; // 1 to 5
    public int? PerformanceAndWashingRating { get; set; } = 5;
    public int? AdvisorBehaviorRating { get; set; } = 5;
    public int? BillingTransparencyRating { get; set; } = 5;
    public string? CustomerComments { get; set; }
    public string RatedVia { get; set; } = "Counter Tablet"; // Counter Tablet, SMS Link, QR Code, Web Portal
    public DateTime RatedDateTime { get; set; } = DateTime.UtcNow;
}
