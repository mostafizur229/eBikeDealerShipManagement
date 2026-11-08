namespace eBike.Domain.Entities;

public class Expenditure
{
    public int ExpenditureID { get; set; }
    public DateTime EntryDate { get; set; }
    public string? Purpose { get; set; }
    public decimal Amount { get; set; }
    public int ExpenseItemID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int CustomerType { get; set; } = 0;
    public string? Remarks { get; set; }
    public int WFStatus { get; set; } = 2; // Approved = 2

    // Helper
    public int Id => ExpenditureID;

    // Navigation properties
    public ExpenseItem? ExpenseItem { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }
}
