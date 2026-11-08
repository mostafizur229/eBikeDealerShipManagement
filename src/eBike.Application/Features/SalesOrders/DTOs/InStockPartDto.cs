namespace eBike.Application.Features.SalesOrders.DTOs;

public class InStockPartDto
{
    public int SDetailID { get; set; }
    public int StockID { get; set; }
    public int ProductID { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int PartsNoID { get; set; }
    public string? PartNo { get; set; }
    public int PLocationID { get; set; }
    public string? LocationName { get; set; }
    public int? CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public int? CompanyID { get; set; }
    public string? CompanyName { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public int GodownID { get; set; }
    public string? GodownName { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal SalesRate { get; set; }
    public decimal MRPRate { get; set; }
    public decimal PRate { get; set; }
    public decimal PWDiscount { get; set; }
    public decimal Mushok { get; set; }
    public string? UnitType { get; set; }
}
