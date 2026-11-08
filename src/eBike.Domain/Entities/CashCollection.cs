namespace eBike.Domain.Entities;

public class CashCollection
{
    public int CashCollectionID { get; set; }
    public int? PaymentType { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public DateTime? EntryDate { get; set; }
    public decimal? Amount { get; set; }
    public string? AccountNo { get; set; }
    public string? MBAccountNo { get; set; }
    public string? BKashNo { get; set; }
    public int? TransactionType { get; set; }
    public int? CustomerID { get; set; }
    public int? SupplierID { get; set; }
    public int ConcernID { get; set; }
    public string? ReceiptNo { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public decimal AdjustAmt { get; set; }
    public decimal BalanceDue { get; set; }
    public string? Remarks { get; set; }
    public decimal InterestAmt { get; set; }
    public string? MRNo { get; set; }
    public int ProjectType { get; set; } = 1;

    public Supplier? Supplier { get; set; }
    public Customer? Customer { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
