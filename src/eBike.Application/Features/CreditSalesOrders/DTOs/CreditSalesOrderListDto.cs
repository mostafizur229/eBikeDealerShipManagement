using eBike.Domain.Enums;

namespace eBike.Application.Features.CreditSalesOrders.DTOs;

public class CreditSalesOrderListDto
{
    public int CreditSalesID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime SalesDate { get; set; }
    public string? LedgerNo { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? ContactNo { get; set; }
    public decimal DueAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal NetAmount { get; set; }
    public decimal DownPayment { get; set; }
    public int NoOfInstallment { get; set; }
    public int Status { get; set; }
    public string StatusName => Enum.IsDefined(typeof(EnumSalesType), Status)
        ? ((EnumSalesType)Status).ToString()
        : Status.ToString();
    public string? MRNo { get; set; }
    public string? REGMRNo { get; set; }
    public string? Remarks { get; set; }
    public int ConcernID { get; set; }
    public int TotalItemsCount { get; set; }
    public decimal TotalQuantity { get; set; }
}
