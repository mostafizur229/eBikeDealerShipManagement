using eBike.Domain.Enums;

namespace eBike.Application.Features.SalesOrders.DTOs;

public class SalesOrderDto
{
    public int SOrderID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal VATPercentage { get; set; }
    public decimal VATAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDPercentage { get; set; }
    public decimal TDAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal? RecAmount { get; set; }
    public decimal? NetDiscount { get; set; }
    public string? Remarks { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? ContactNo { get; set; }
    public string? CustomerAddress { get; set; }
    public decimal CustomerTotalDue { get; set; }
    public decimal AdjAmount { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public string StatusName => Enum.IsDefined(typeof(EnumSalesType), Status)
        ? ((EnumSalesType)Status).ToString()
        : Status.ToString();
    public int ConcernID { get; set; }
    public decimal TotalOffer { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalInstAmt { get; set; }
    public decimal TOfferAmt { get; set; }
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public string? VATChallanPotroNo { get; set; }
    public int SOType { get; set; }

    public List<SalesOrderDetailDto> Details { get; set; } = new();
}

public class SalesOrderDetailDto
{
    public int SOrderDetailID { get; set; }
    public int ProductID { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int SDetailID { get; set; }
    public string? IMENO { get; set; } // Chassis No
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public int GodownID { get; set; }
    public string? GodownName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal PPDPercentage { get; set; }
    public decimal PPDAmount { get; set; }
    public decimal UTAmount { get; set; }
    public decimal MPRate { get; set; }
    public decimal PRate { get; set; }
    public decimal SRate { get; set; }
    public decimal PPOffer { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public int RegType { get; set; }
    public string RegTypeName => Enum.IsDefined(typeof(EnumRegType), RegType)
        ? ((EnumRegType)RegType).ToString()
        : RegType.ToString();
    public decimal RegAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmt { get; set; }
    public decimal InsuranceAmt { get; set; }
    public string? Remarks { get; set; }
}
