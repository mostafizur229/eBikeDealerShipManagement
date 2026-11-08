namespace eBike.Application.Features.CreditInterestPercentages.DTOs;

public class CreditInterestPercentageDto
{
    public int IntPercentageId { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public DateTime? EffectDate { get; set; }
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
}
