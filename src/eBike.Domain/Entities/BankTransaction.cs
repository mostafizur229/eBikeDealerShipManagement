namespace eBike.Domain.Entities;

public class BankTransaction
{
    public int BankTranID { get; set; }
    public DateTime TranDate { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public decimal Amount { get; set; }
    public int? BankID { get; set; }
    public int? CustomerID { get; set; }
    public int? SupplierID { get; set; }
    public int? AnotherBankID { get; set; }
    public string? ChecqueNo { get; set; }
    public string? Remarks { get; set; }
    public int? ConcernID { get; set; }
    public int WFStatus { get; set; } = 2;
    public int? ExpenseItemID { get; set; }
    public int? SIHID { get; set; }
    public int Id => BankTranID;
    public Bank? Bank { get; set; }
    public Bank? AnotherBank { get; set; }
    public Customer? Customer { get; set; }
    public Supplier? Supplier { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
