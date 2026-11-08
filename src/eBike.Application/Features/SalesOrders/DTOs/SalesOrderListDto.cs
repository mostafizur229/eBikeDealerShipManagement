using eBike.Domain.Enums;

namespace eBike.Application.Features.SalesOrders.DTOs;

public class SalesOrderListDto
{
    public int SOrderID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? ContactNo { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? RecAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public decimal? NetDiscount { get; set; }
    public decimal AdjAmount { get; set; }
    public int Status { get; set; }
    public string StatusName => Enum.IsDefined(typeof(EnumSalesType), Status)
        ? ((EnumSalesType)Status).ToString()
        : Status.ToString();
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public string? Remarks { get; set; }
    public int ConcernID { get; set; }
    public int TotalItemsCount { get; set; }
    public decimal TotalQuantity { get; set; }
}
