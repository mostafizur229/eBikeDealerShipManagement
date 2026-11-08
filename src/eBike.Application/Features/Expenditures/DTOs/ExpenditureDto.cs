namespace eBike.Application.Features.Expenditures.DTOs;

public class ExpenditureDto
{
    public int ExpenditureID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public decimal Amount { get; set; }
    public string? Purpose { get; set; }
    public int ExpenseItemID { get; set; }
    public string? ExpenseItemName { get; set; }
    public string? ExpenseItemCode { get; set; }
    public int ExpenseItemStatus { get; set; }
    public string ExpenseItemStatusName { get; set; } = string.Empty;
    public int CustomerType { get; set; }
    public string? Remarks { get; set; }
    public int WFStatus { get; set; }
    public string WFStatusName { get; set; } = string.Empty;
    public int ConcernID { get; set; }
    public string? ConcernName { get; set; }
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? CreateDate { get; set; }
}

public class ExpenditureListDto
{
    public int ExpenditureID { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public decimal Amount { get; set; }
    public string? Purpose { get; set; }
    public int ExpenseItemID { get; set; }
    public string? ExpenseItemName { get; set; }
    public int ExpenseItemStatus { get; set; }
    public string ExpenseItemStatusName { get; set; } = string.Empty;
    public int WFStatus { get; set; }
    public string WFStatusName { get; set; } = string.Empty;
    public int ConcernID { get; set; }
    public string? CreatedByName { get; set; }
}
