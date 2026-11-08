using eBike.Domain.Enums;

namespace eBike.Application.Features.CashCollections.DTOs;

public class CashCollectionDto
{
    public int CashCollectionID { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; }
    public int? CustomerID { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerContactNo { get; set; }
    public decimal Amount { get; set; }
    public decimal AdjustAmt { get; set; }
    public decimal InterestAmt { get; set; }
    public decimal BalanceDue { get; set; }
    public int PaymentType { get; set; }
    public string PaymentTypeName => Enum.IsDefined(typeof(EnumPayType), PaymentType)
        ? ((EnumPayType)PaymentType).ToString()
        : PaymentType.ToString();
    public string? AccountNo { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? MBAccountNo { get; set; }
    public string? BKashNo { get; set; }
    public string? MRNo { get; set; }
    public string? Remarks { get; set; }
    public DateTime? RemindDate { get; set; }
    public int TransactionType { get; set; }
    public string TransactionTypeName => Enum.IsDefined(typeof(EnumTranType), TransactionType)
        ? ((EnumTranType)TransactionType).ToString()
        : TransactionType.ToString();
    public int ProjectType { get; set; } = 1;
    public string ProjectTypeName => ProjectType == 2 ? "AutoParts" : "Motorcycle";
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
}
