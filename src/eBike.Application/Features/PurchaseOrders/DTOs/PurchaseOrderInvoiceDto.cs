namespace eBike.Application.Features.PurchaseOrders.DTOs;

public class PurchaseOrderInvoiceDto
{
    public PurchaseOrderCompanyDto Company { get; set; } = new();
    public PurchaseOrderSupplierDto Supplier { get; set; } = new();
    public PurchaseOrderMetadataDto Order { get; set; } = new();
    public PurchaseOrderFinancialsDto Financials { get; set; } = new();
    public List<PurchaseOrderInvoiceItemDto> Items { get; set; } = new();
}

public class PurchaseOrderCompanyDto
{
    public int ConcernID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? TelephoneNo { get; set; }
    public string? EmailAddress { get; set; }
    public string? WebAddress { get; set; }
    public string? VATRegistrationNo { get; set; }
}

public class PurchaseOrderSupplierDto
{
    public int SupplierID { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
    public decimal TotalDue { get; set; }
}

public class PurchaseOrderMetadataDto
{
    public int POrderID { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int POType { get; set; }
    public string POTypeName { get; set; } = "Vehicle"; // "Vehicle" or "Auto Parts"
    public string ReportHeader { get; set; } = "Purchase Order";
    public int Status { get; set; }
    public string StatusName { get; set; } = "Purchase";
    public int? IsDamageOrder { get; set; }
    public string? Remarks { get; set; }
    public DateTime PrintDate { get; set; } = DateTime.Now;
    public int? CreatedByID { get; set; }
    public string? CreatedByName { get; set; }
}

public class PurchaseOrderFinancialsDto
{
    public decimal GrandTotal { get; set; }
    public decimal TDiscount { get; set; }
    public decimal PPDisAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal LaborCost { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public string InWordsTotal { get; set; } = string.Empty;
    public string InWordsPaid { get; set; } = string.Empty;
}

public class PurchaseOrderInvoiceItemDto
{
    public int Sl { get; set; }
    public int POrderDetailID { get; set; }
    public int ProductID { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? CompanyName { get; set; }
    public string? ColorName { get; set; }
    public string? GodownName { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DisPer { get; set; }
    public decimal DisAmt { get; set; }
    public decimal ExtraDisPer { get; set; }
    public decimal ExtraDisAmt { get; set; }
    public decimal TotalAmt { get; set; }

    public decimal DealerUnitPrice { get; set; }
    public decimal HolTax { get; set; }
    public List<string> ChassisNumbers { get; set; } = new();
    public List<string> EngineNumbers { get; set; } = new();
    public string ChassisNumbersText { get; set; } = string.Empty;
    public string EngineNumbersText { get; set; } = string.Empty;

    public string? PartNo { get; set; }
    public string? LocationName { get; set; }
}
