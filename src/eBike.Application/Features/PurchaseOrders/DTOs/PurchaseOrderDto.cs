namespace eBike.Application.Features.PurchaseOrders.DTOs;

public class PurchaseOrderDto
{
    public int POrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public int SupplierID { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierContact { get; set; }
    public decimal SupplierTotalDue { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDiscount { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal PPDisAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal LaborCost { get; set; }
    public decimal AdjAmount { get; set; }
    public int ConcernID { get; set; }
    public int? IsDamageOrder { get; set; }
    public string? Remarks { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalTax { get; set; }
    public int POType { get; set; }
    public bool HasSoldProducts { get; set; }

    public List<PurchaseOrderDetailDto> Details { get; set; } = new();
}

public class PurchaseOrderDetailDto
{
    public int POrderDetailID { get; set; }
    public int ProductID { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TAmount { get; set; }
    public decimal PPDISPer { get; set; }
    public decimal PPDISAmt { get; set; }
    public decimal MRPRate { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public decimal SalesRate { get; set; }
    public decimal ExtraPPDISPer { get; set; }
    public decimal ExtraPPDISAmt { get; set; }
    public decimal PPOFFER { get; set; }
    public decimal CreditSalesRate { get; set; }
    public decimal CRSalesRate12Month { get; set; }
    public decimal CRSalesRate3Month { get; set; }
    public int GodownID { get; set; }
    public string? GodownName { get; set; }
    public decimal Mushok { get; set; }
    public decimal MRPWMushok { get; set; }
    public decimal DPRate { get; set; }
    public decimal RecQuantity { get; set; }
    public int DOrderDetailId { get; set; }
    public decimal PWTaxAmt { get; set; }

    public List<POProductDetailDto> Serials { get; set; } = new();
}

public class POProductDetailDto
{
    public int POPDID { get; set; }
    public int ProductID { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public string IMENO { get; set; } = string.Empty; // Chassis No
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public int? DamagePOPDID { get; set; }
    public int? IsDamageReplaced { get; set; }
    public string? StatusName { get; set; } // Stock / Sold / Return
}

public class ChassisDuplicateCheckDto
{
    public string IMENO { get; set; } = string.Empty;
    public bool IsDuplicate { get; set; }
    public string Message { get; set; } = string.Empty;
}
