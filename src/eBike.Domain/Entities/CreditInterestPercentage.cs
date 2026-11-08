namespace eBike.Domain.Entities;

public class CreditInterestPercentage
{
    public int IntPercentageID { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int ConcernID { get; set; }
    public DateTime? EffectDate { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
