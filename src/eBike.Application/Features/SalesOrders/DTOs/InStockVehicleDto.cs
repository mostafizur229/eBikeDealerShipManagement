namespace eBike.Application.Features.SalesOrders.DTOs;

public class InStockVehicleDto
{
    public int SDetailID { get; set; }
    public int ProductID { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? CompanyName { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public int GodownID { get; set; }
    public string? GodownName { get; set; }
    public string IMENO { get; set; } = string.Empty; // Chassis No
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public decimal MRPRate { get; set; }
    public decimal PRate { get; set; }
    public decimal SalesRate { get; set; }
    public decimal CreditSalesRate { get; set; }
    public decimal CRSalesRate12Month { get; set; }
    public decimal CRSalesRate3Month { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public decimal StockQuantity { get; set; } = 1;
}
