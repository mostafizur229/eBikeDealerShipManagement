namespace eBike.Domain.Entities;

public class CreditSalesSchedule
{
    public int CSScheduleID { get; set; }
    public DateTime MonthDate { get; set; }
    public decimal Balance { get; set; }
    public decimal InstallmentAmt { get; set; }
    public DateTime PaymentDate { get; set; }
    public int CreditSalesID { get; set; }
    public string PaymentStatus { get; set; } = "Due";
    public decimal? InterestAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public int ScheduleNo { get; set; }
    public string? Remarks { get; set; }
    public int IsUnExpected { get; set; }
    public decimal HireValue { get; set; }
    public decimal NetValue { get; set; }
    public DateTime? RemindDate { get; set; }
    public decimal ExpectedInstallment { get; set; }
    public decimal LastPayAdjust { get; set; }
    public int BankTransID { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public decimal LastPayAdjustment { get; set; }
    public string? MRNO { get; set; }

    public CreditSale? CreditSale { get; set; }
}
