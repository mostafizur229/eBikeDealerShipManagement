namespace eBike.Application.Features.Registrations.DTOs;

public class CustomerUnregisteredBikeDto
{
    public int StockDetailId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? CC { get; set; }
    public decimal ParsedCC { get; set; }
    public string? ChassisNo { get; set; }
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public int? ColorId { get; set; }
    public string? ColorName { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime? SalesDate { get; set; }
    public string? InvoiceNo { get; set; }
    public string SaleType { get; set; } = string.Empty; // "Cash" or "Credit"
    public int? SOrderDetailId { get; set; }
    public int? CreditSaleDetailsId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
}

