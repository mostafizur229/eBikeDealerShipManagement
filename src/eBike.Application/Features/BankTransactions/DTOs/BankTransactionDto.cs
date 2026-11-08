namespace eBike.Application.Features.BankTransactions.DTOs;

public class BankTransactionDto
{
    public int BankTranID { get; set; }
    public DateTime TranDate { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? BankID { get; set; }
    public string? BankName { get; set; }
    public string? AccountNo { get; set; }
    public string? AccountName { get; set; }
    public int? AnotherBankID { get; set; }
    public string? AnotherBankName { get; set; }
    public string? AnotherAccountNo { get; set; }
    public int? CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public string? ChecqueNo { get; set; }
    public string? Remarks { get; set; }
    public int? ExpenseItemID { get; set; }
    public int? SIHID { get; set; }
    public int WFStatus { get; set; }
    public string WFStatusName { get; set; } = string.Empty;
    public int? ConcernID { get; set; }
    public string? ConcernName { get; set; }
}

public class BankTransactionListDto
{
    public int BankTranID { get; set; }
    public DateTime TranDate { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? BankID { get; set; }
    public string? BankName { get; set; }
    public string? AccountNo { get; set; }
    public int? AnotherBankID { get; set; }
    public string? AnotherBankName { get; set; }
    public int? CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public string? ChecqueNo { get; set; }
    public string? Remarks { get; set; }
    public int WFStatus { get; set; }
    public string WFStatusName { get; set; } = string.Empty;
    public int? ConcernID { get; set; }
}
