namespace eBike.Domain.Entities;

public class ExpenseItem
{
    public int ExpenseItemID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // Head Name
    public int Status { get; set; } = 1; // 1: Expense, 2: Income (EnumCompanyTransaction)
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Helpers
    public int Id => ExpenseItemID;
    public string Name => Description;

    // Navigation properties
    public SisterConcern? SisterConcern { get; set; }
}
