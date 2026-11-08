namespace eBike.Domain.Entities;

public class CreditSale
{
    public int CreditSalesID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public decimal TSalesAmt { get; set; }
    public int NoOfInstallment { get; set; }
    public decimal IntallmentPrinciple { get; set; }
    public DateTime IssueDate { get; set; }
    public string? UserName { get; set; }
    public decimal Remaining { get; set; }
    public DateTime SalesDate { get; set; }
    public decimal DownPayment { get; set; }
    public decimal? WInterestAmt { get; set; }
    public decimal? FixedAmt { get; set; }
    public int IsStatus { get; set; }
    public int UnExInstallment { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount { get; set; }
    public bool ISUnexpected { get; set; }
    public string? Remarks { get; set; }
    public int Status { get; set; }
    public decimal VATPercentage { get; set; }
    public decimal VATAmount { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalOffer { get; set; }
    public decimal PGrandTotal { get; set; }
    public decimal PenaltyInterest { get; set; }
    public decimal LastPayAdjAmt { get; set; }
    public string? InstallmentPeriod { get; set; }
    public int IsReturn { get; set; }
    public int? BankTransID { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public decimal MergeTotalSales { get; set; }
    public int IsOffer { get; set; }
    public decimal TOfferAmt { get; set; }
    public decimal TotalMushok { get; set; }
    public string? BChallanNo { get; set; }
    public DateTime? ChallanDate { get; set; }
    public decimal TExAmount { get; set; }
    public string? MRNo { get; set; }
    public string? CBankName { get; set; }
    public string? CBBranch { get; set; }
    public string? CBChequeNo { get; set; }
    public string? VATChallanPotroNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? CBNID { get; set; }
    public string? REGMRNo { get; set; }

    public Customer? Customer { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<CreditSaleDetail> CreditSaleDetails { get; set; } = new List<CreditSaleDetail>();
    public ICollection<CreditSalesSchedule> CreditSalesSchedules { get; set; } = new List<CreditSalesSchedule>();
}
