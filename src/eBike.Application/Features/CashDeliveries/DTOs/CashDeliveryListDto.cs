using eBike.Domain.Enums;

namespace eBike.Application.Features.CashDeliveries.DTOs;

public class CashDeliveryListDto
{
    public int CashCollectionID { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }
    public string? AccountNo { get; set; }
    public decimal Amount { get; set; }
    public decimal AdjustAmt { get; set; }
    public decimal BalanceDue { get; set; }
    public int PaymentType { get; set; }
    public string PaymentTypeName => Enum.IsDefined(typeof(EnumPayType), PaymentType)
        ? ((EnumPayType)PaymentType).ToString()
        : PaymentType.ToString();
    public int TransactionType { get; set; }
    public string TransactionTypeName => Enum.IsDefined(typeof(EnumTranType), TransactionType)
        ? ((EnumTranType)TransactionType).ToString()
        : TransactionType.ToString();
    public string? Remarks { get; set; }
}
