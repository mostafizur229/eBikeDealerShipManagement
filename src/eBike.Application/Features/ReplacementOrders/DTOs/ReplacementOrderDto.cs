namespace eBike.Application.Features.ReplacementOrders.DTOs;

public class ReplacementOrderListDto
{
    public int SOrderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public decimal DamageTotalAmount { get; set; }
    public decimal ReplaceTotalAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AdjAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RecAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int ItemCount { get; set; }
    public string Status { get; set; } = "Replacement";
}

public class ReplacementOrderDto
{
    public int SOrderId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? CustomerAddress { get; set; }
    public decimal DamageTotalAmount { get; set; }
    public decimal ReplaceTotalAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AdjAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RecAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public string? Remarks { get; set; }
    public DateTime? CreateDate { get; set; }
    public List<ReplacementOrderDetailDto> Items { get; set; } = new();
}

public class ReplacementOrderDetailDto
{
    public int SOrderDetailId { get; set; }
    public int SOrderId { get; set; }
    public int DamageProductId { get; set; }
    public string? DamageProductCode { get; set; }
    public string? DamageProductName { get; set; }
    public int DamageStockDetailId { get; set; }
    public string? DamageIMENO { get; set; }
    public string? DamageEngineNo { get; set; }
    public string? DamageColorName { get; set; }
    public decimal DamageUnitPrice { get; set; }
    public int ReplaceProductType { get; set; }
    public string ReplaceProductTypeName => ReplaceProductType == 1 ? "Damage" : "Good Condition";
    public int NewProductId { get; set; }
    public string? NewProductCode { get; set; }
    public string? NewProductName { get; set; }
    public int NewStockDetailId { get; set; }
    public string? NewIMENO { get; set; }
    public string? NewEngineNo { get; set; }
    public string? NewColorName { get; set; }
    public decimal NewUnitPrice { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Remarks { get; set; }
}

public class CustomerEligibleProductDto
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
    public int? GodownId { get; set; }
    public string? GodownName { get; set; }
    public decimal SoldUnitPrice { get; set; }
    public decimal SoldQuantity { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime InvoiceDate { get; set; }
}

public class AvailableStockProductDto
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
    public decimal SRate { get; set; }
    public decimal MRPRate { get; set; }
    public decimal Quantity { get; set; }
}
