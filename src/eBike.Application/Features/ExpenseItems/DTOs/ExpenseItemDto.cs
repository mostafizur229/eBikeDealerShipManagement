namespace eBike.Application.Features.ExpenseItems.DTOs;

public class ExpenseItemDto
{
    public int ExpenseItemID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; } // 1: Expense, 2: Income
    public string StatusName { get; set; } = string.Empty;
    public int ConcernID { get; set; }
    public string? ConcernName { get; set; }
}

public class ExpenseItemListDto
{
    public int ExpenseItemID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int ConcernID { get; set; }
}
