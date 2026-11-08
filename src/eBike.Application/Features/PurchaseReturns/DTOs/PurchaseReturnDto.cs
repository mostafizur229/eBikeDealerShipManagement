namespace eBike.Application.Features.PurchaseReturns.DTOs;

public class PurchaseReturnListDto
{
    public int POrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public int SupplierID { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierContact { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = "Return";
    public int ItemCount { get; set; }
    public string? Remarks { get; set; }
}

public class PurchaseReturnDto
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
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = "Return";
    public string? Remarks { get; set; }
    public int ConcernID { get; set; }

    public List<PurchaseReturnDetailDto> Details { get; set; } = new();
}

public class PurchaseReturnDetailDto
{
    public int POrderDetailID { get; set; }
    public int ProductID { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TAmount { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public int GodownID { get; set; }
    public string? GodownName { get; set; }

    public List<PurchaseReturnSerialDto> Serials { get; set; } = new();
}

public class PurchaseReturnSerialDto
{
    public int POPDID { get; set; }
    public int ProductID { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public string IMENO { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public int? ReturnSDetailID { get; set; }
    public decimal ReturnQty { get; set; }
}
