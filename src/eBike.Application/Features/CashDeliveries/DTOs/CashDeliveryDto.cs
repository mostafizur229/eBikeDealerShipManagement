using eBike.Domain.Enums;

namespace eBike.Application.Features.CashDeliveries.DTOs;

public class CashDeliveryDto
{
    public int CashCollectionID { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }
    public decimal Amount { get; set; }
    public decimal AdjustAmt { get; set; }
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
    public string? Remarks { get; set; }
    public int TransactionType { get; set; }
    public string TransactionTypeName => Enum.IsDefined(typeof(EnumTranType), TransactionType)
        ? ((EnumTranType)TransactionType).ToString()
        : TransactionType.ToString();
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
}
