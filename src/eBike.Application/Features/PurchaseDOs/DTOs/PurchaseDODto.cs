namespace eBike.Application.Features.PurchaseDOs.DTOs;

public class PurchaseDOListDto
{
    public int DOID { get; set; }
    public DateTime Date { get; set; }
    public string DONo { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public int SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public int Status { get; set; }
    public string StatusName => Status switch
    {
        1 => "DO",
        2 => "Return",
        3 => "Complete",
        _ => "Unknown"
    };
    public string? Remarks { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal PaidAmt { get; set; }
    public decimal PaymentDue => TotalAmt - PaidAmt;
    public decimal NetDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    public int ConcernID { get; set; }
    public int TotalItemsCount { get; set; }
}

public class PurchaseDODto
{
    public int DOID { get; set; }
    public DateTime Date { get; set; }
    public string DONo { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public int SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public int Status { get; set; }
    public string? Remarks { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal PaidAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    public int ConcernID { get; set; }
    public List<PurchaseDODetailDto> Details { get; set; } = new();
}

public class PurchaseDODetailDto
{
    public int DODID { get; set; }
    public int DOID { get; set; }
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int ColorID { get; set; }
    public string? ColorName { get; set; }
    public decimal DOQty { get; set; }
    public decimal GivenQty { get; set; }
    public decimal MRP { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal PPDisPercent { get; set; }
    public decimal PPDisAmt { get; set; }
    public decimal PPTaxAmount { get; set; }
}
