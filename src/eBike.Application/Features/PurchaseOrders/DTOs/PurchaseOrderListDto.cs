using eBike.Domain.Enums;

namespace eBike.Application.Features.PurchaseOrders.DTOs;

public class PurchaseOrderListDto
{
    public int POrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public int SupplierID { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDiscount { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName => Enum.IsDefined(typeof(EnumPurchaseType), Status)
        ? ((EnumPurchaseType)Status).ToString()
        : Status.ToString();
    public decimal PPDisAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal LaborCost { get; set; }
    public decimal AdjAmount { get; set; }
    public int ConcernID { get; set; }
    public int? IsDamageOrder { get; set; }
    public string? Remarks { get; set; }
    public int TotalItemsCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public bool HasSoldProducts { get; set; }
}
