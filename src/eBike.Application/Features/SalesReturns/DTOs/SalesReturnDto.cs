namespace eBike.Application.Features.SalesReturns.DTOs;

public class SalesReturnListDto
{
    public int SOrderID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerContact { get; set; }
    public string? CustomerAddress { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? RecAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = "Sales Return";
    public int ItemCount { get; set; }
    public string? Remarks { get; set; }
}

public class SalesReturnDto
{
    public int SOrderID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerContact { get; set; }
    public string? CustomerAddress { get; set; }
    public decimal CustomerTotalDue { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? RecAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = "Sales Return";
    public string? Remarks { get; set; }
    public int ConcernID { get; set; }

    public List<SalesReturnDetailDto> Details { get; set; } = new();
}

public class SalesReturnDetailDto
{
    public int SOrderDetailID { get; set; }
    public int ProductID { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int? StockDetailID { get; set; }
    public string? IMENO { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public int GodownID { get; set; }
    public string? GodownName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UTAmount { get; set; }
}

public class CustomerSoldProductDto
{
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int StockDetailId { get; set; }
    public string? IMENO { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public int GodownId { get; set; }
    public string? GodownName { get; set; }
    public decimal SoldUnitPrice { get; set; }
    public decimal SoldQuantity { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
}
