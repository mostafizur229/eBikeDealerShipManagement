namespace eBike.Domain.Entities;

public class Bank
{
    public int BankID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalAmount { get; set; }
    public int ConcernID { get; set; }
    public int? IsAdvancedDueLimitApplicable { get; set; }
    public decimal? AdvancedAmountLimit { get; set; }
    public int Id => BankID;
    public SisterConcern? SisterConcern { get; set; }
}
