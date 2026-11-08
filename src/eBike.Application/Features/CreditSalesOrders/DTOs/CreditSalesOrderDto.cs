using eBike.Domain.Enums;

namespace eBike.Application.Features.CreditSalesOrders.DTOs;

public class CreditSalesOrderDto
{
    public int CreditSalesID { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime SalesDate { get; set; }
    public DateTime IssueDate { get; set; }
    public int CustomerID { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? ContactNo { get; set; }
    public string? CustomerAddress { get; set; }
    public decimal CustomerCreditDue { get; set; }
    public string? LedgerNo { get; set; }
    public string? MRNo { get; set; }
    public string? REGMRNo { get; set; }
    public decimal VATPercentage { get; set; }
    public decimal VATAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal DownPayment { get; set; }
    public decimal Remaining { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalOffer { get; set; }
    public decimal TotalMushok { get; set; }
    public int NoOfInstallment { get; set; }
    public decimal IntallmentPrinciple { get; set; }
    public string? InstallmentPeriod { get; set; }
    public decimal LastPayAdjAmt { get; set; }
    public string? Remarks { get; set; }
    public int Status { get; set; }
    public string StatusName => Enum.IsDefined(typeof(EnumSalesType), Status)
        ? ((EnumSalesType)Status).ToString()
        : Status.ToString();
    public int ConcernID { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public int? BankTransID { get; set; }
    public string? CBankName { get; set; }
    public string? CBBranch { get; set; }
    public string? CBChequeNo { get; set; }
    public string? CBNID { get; set; }
    public string? VATChallanPotroNo { get; set; }

    public List<CreditSalesOrderDetailDto> Details { get; set; } = new();
    public List<CreditSalesScheduleDto> Schedules { get; set; } = new();
}

public class CreditSalesOrderDetailDto
{
    public int CreditSaleDetailsID { get; set; }
    public int ProductID { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public int StockDetailID { get; set; }
    public string? IMENO { get; set; } // Chassis No
    public string? EngineNo { get; set; }
    public string? KeyNo { get; set; }
    public int? ColorID { get; set; }
    public string? ColorName { get; set; }
    public int? GodownID { get; set; }
    public string? GodownName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UTAmount { get; set; }
    public decimal MPRate { get; set; }
    public decimal PRate { get; set; }
    public decimal SRate { get; set; }
    public decimal PPOffer { get; set; }
    public decimal IntPercentage { get; set; }
    public decimal IntTotalAmt { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public int RegType { get; set; }
    public string RegTypeName => Enum.IsDefined(typeof(EnumRegType), RegType)
        ? ((EnumRegType)RegType).ToString()
        : RegType.ToString();
    public decimal RegAmount { get; set; }
    public decimal InsuranceAmt { get; set; }
    public decimal OfferAmt { get; set; }
}

public class CreditSalesScheduleDto
{
    public int CSScheduleID { get; set; }
    public int ScheduleNo { get; set; }
    public DateTime MonthDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal Balance { get; set; }
    public decimal InstallmentAmt { get; set; }
    public decimal ClosingBalance { get; set; }
    public string PaymentStatus { get; set; } = "Due";
    public decimal ExpectedInstallment { get; set; }
    public decimal HireValue { get; set; }
    public decimal NetValue { get; set; }
    public decimal LastPayAdjust { get; set; }
    public string? MRNO { get; set; }
    public string? Remarks { get; set; }
    public DateTime? RemindDate { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
}
