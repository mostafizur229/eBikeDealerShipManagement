namespace eBike.Domain.Entities;

public class SOrder
{
    public int SOrderID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal VATPercentage { get; set; }
    public decimal VATAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDPercentage { get; set; }
    public decimal TDAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal? RecAmount { get; set; }
    public decimal? NetDiscount { get; set; }
    public string? Remarks { get; set; }
    public int CustomerID { get; set; }
    public decimal AdjAmount { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public decimal TotalOffer { get; set; }
    public decimal PGrandTotal { get; set; }
    public int IsReplacement { get; set; }
    public int BankTransID { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public int IsOffer { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalInstAmt { get; set; }
    public decimal TOfferAmt { get; set; }
    public DateTime? ChallanDate { get; set; }
    public string? BChallanNo { get; set; }
    public string? MRNo { get; set; }
    public string? VATChallanPotroNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public int SOType { get; set; } = 1;
    public int PenaltyStatus { get; set; }
    public DateTime? PenaltyDate { get; set; }
    public decimal PenaltyAmount { get; set; }
    public int? JobCardID { get; set; }

    public Customer? Customer { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<SOrderDetail> SOrderDetails { get; set; } = new List<SOrderDetail>();
}
