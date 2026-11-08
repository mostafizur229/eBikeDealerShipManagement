namespace eBike.Application.Common.Interfaces;

public class CreditSalesOrderExecutionModel
{
    public string InvoiceNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal TSalesAmt { get; set; }
    public int NoOfInstallment { get; set; }
    public decimal InstallmentPrinciple { get; set; }
    public DateTime IssueDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Remaining { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public DateTime SalesDate { get; set; }
    public decimal DownPayment { get; set; }
    public decimal WInterestAmt { get; set; }
    public decimal FixedAmount { get; set; }
    public int IsStatus { get; set; } = 1;
    public int UnExInstallment { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount { get; set; }
    public bool IsUnExpected { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalDue { get; set; }
    public int ConcernId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public decimal TotalOffer { get; set; }
    public string? InstallmentPeriod { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public string? MRNo { get; set; }
    public string? CBankName { get; set; }
    public string? CBBranch { get; set; }
    public string? CBChequeNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? CBNID { get; set; }
    public string? REGMRNo { get; set; }

    public int? BankID { get; set; }
    public int? CardTypeID { get; set; }

    public List<CreditSalesOrderDetailExecutionItem> Details { get; set; } = new();
    public List<CreditSalesScheduleExecutionItem> Schedules { get; set; } = new();
    public BankTransExecutionItem? BankTrans { get; set; }
}

public class CreditSalesOrderDetailExecutionItem
{
    public int ProductId { get; set; }
    public int StockDetailId { get; set; }
    public int ColorId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal UTAmount { get; set; }
    public decimal MpRateTotal { get; set; }
    public decimal MrpRate { get; set; }
    public decimal PPOffer { get; set; }
    public decimal IntPercentage { get; set; }
    public decimal IntTotalAmt { get; set; }
    public int Compressor { get; set; } // GodownID
    public int Motor { get; set; }
    public int Panel { get; set; }
    public int Spareparts { get; set; }
    public int Service { get; set; }
    public int CreditSalesDetailsId { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public decimal RegAmt { get; set; }
    public int OfferId { get; set; }
    public decimal OfferAmt { get; set; }
    public decimal InsuranceAmt { get; set; }
    public int RegType { get; set; }
}

public class CreditSalesScheduleExecutionItem
{
    public int CSScheduleID { get; set; }
    public DateTime MonthDate { get; set; }
    public decimal Balance { get; set; }
    public decimal InstallmentAmt { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string PaymentStatus { get; set; } = "Due";
    public decimal InterestAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public int ScheduleNo { get; set; }
    public string? Remarks { get; set; }
    public int IsUnExpected { get; set; }
    public decimal HireValue { get; set; }
    public decimal NetValue { get; set; }
    public decimal ExpectedInstallment { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public int BankTransID { get; set; }
    public string? MRNO { get; set; }
    public decimal LastPayAdjustment { get; set; }
}

public class CreditSalesInstallmentPaymentModel
{
    public int SalesOrderId { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal LastPayAdjustment { get; set; }
    public int CardTypeSetupID { get; set; }
    public int? BankID { get; set; }
    public int? CardTypeID { get; set; }
    public decimal CardPaidAmount { get; set; }
    public decimal DepositChargePercent { get; set; }
    public List<CreditSalesScheduleExecutionItem> Schedules { get; set; } = new();
    public BankTransExecutionItem? BankTrans { get; set; }
}

public interface ICreditSalesOrderExecutionService
{
    Task<bool> AddCreditSalesOrderAsync(CreditSalesOrderExecutionModel model, CancellationToken cancellationToken = default);
    Task<bool> InstallmentPaymentAsync(CreditSalesInstallmentPaymentModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteCreditSalesOrderAsync(int salesOrderId, int userId, CancellationToken cancellationToken = default);
    Task<string> GetNextInvoiceNoAsync(int concernId, CancellationToken cancellationToken = default);
    Task<bool> IsInvoiceNoExistsAsync(string invoiceNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default);
    Task<bool> IsMRNoExistsAsync(string mrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default);
    Task<bool> IsREGMRNoExistsAsync(string regMrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default);
    Task<bool> IsChassisSoldAsync(int stockDetailId, CancellationToken cancellationToken = default);
    Task<(int CardTypeSetupId, decimal DepositChargePercent)> GetCardSetupAsync(int bankId, int cardTypeId, CancellationToken cancellationToken = default);
}
