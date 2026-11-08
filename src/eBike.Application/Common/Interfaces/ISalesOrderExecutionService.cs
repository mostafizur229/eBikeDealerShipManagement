namespace eBike.Application.Common.Interfaces;

public class SalesOrderExecutionModel
{
    public DateTime InvoiceDate { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public decimal VatPercentage { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDiscountPercentage { get; set; }
    public decimal TDiscountAmount { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDue { get; set; }
    public decimal AdjAmount { get; set; }
    public int Status { get; set; } = 1;
    public int CustomerId { get; set; }
    public int ConcernId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public decimal TotalOffer { get; set; }
    public decimal NetDiscount { get; set; }
    public string? Remarks { get; set; }
    public decimal CardPaidAmount { get; set; }
    public int CardTypeSetupID { get; set; }
    public decimal DepositChargePercent { get; set; }
    public int IsOffer { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalInstAmt { get; set; }
    public decimal TOfferAmt { get; set; }
    public string? MRNo { get; set; }
    public string? LedgerNo { get; set; }
    public string? REGMRNo { get; set; }
    public int SOType { get; set; } = 1;
    public DateTime? PenaltyDate { get; set; }
    public decimal PenaltyAmount { get; set; }
    public int PenaltyStatus { get; set; }
    public decimal LabourCost { get; set; }
    public DateTime? RemindDate { get; set; }
    public int? BankID { get; set; }
    public int? CardTypeID { get; set; }

    public List<SalesOrderDetailExecutionItem> Details { get; set; } = new();
    public BankTransExecutionItem? BankTrans { get; set; }
}

public class SalesOrderDetailExecutionItem
{
    public int SOrderDetailID { get; set; }
    public int ProductId { get; set; }
    public int StockDetailId { get; set; }
    public int? ColorId { get; set; }
    public int Status { get; set; } = 1;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TAmount { get; set; }
    public decimal PPDisPer { get; set; }
    public decimal PPDisAmt { get; set; }
    public decimal MrpRate { get; set; }
    public decimal PPOffer { get; set; }
    public int Compressor { get; set; } // GodownID
    public int Motor { get; set; }      // RegType
    public int Panel { get; set; }
    public int Spareparts { get; set; }
    public int Service { get; set; }
    public decimal RQuantity { get; set; }
    public int ReplaceProductType { get; set; }
    public decimal PPVat { get; set; }
    public decimal OfferAmt { get; set; }
    public decimal RegAmount { get; set; }
    public int OfferID { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmt { get; set; }
    public decimal InsuranceAmt { get; set; }
}

public class BankTransExecutionItem
{
    public DateTime TranDate { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public int TransactionType { get; set; } = 1; // Deposit
    public decimal Amount { get; set; }
    public int BankID { get; set; }
    public int CustomerID { get; set; }
    public int SupplierID { get; set; }
    public int AnotherBankID { get; set; }
    public string ChecqueNo { get; set; } = string.Empty;
    public string Remarks { get; set; } = "Card payment deposit transactions";
    public int ConcernID { get; set; }
}

public interface ISalesOrderExecutionService
{
    Task<bool> AddSalesOrderAsync(SalesOrderExecutionModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdateSalesOrderAsync(int salesOrderId, int userId, SalesOrderExecutionModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteSalesOrderAsync(int salesOrderId, int userId, CancellationToken cancellationToken = default);
    Task<string> GetNextInvoiceNoAsync(int concernId, int soType = 1, CancellationToken cancellationToken = default);
    Task<bool> IsChassisSoldAsync(int stockDetailId, CancellationToken cancellationToken = default);
    Task<bool> IsInvoiceNoExistsAsync(string invoiceNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default);
    Task<bool> IsMRNoExistsAsync(string mrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default);
    Task<bool> IsREGMRNoExistsAsync(string regMrNo, int concernId, int? excludeSalesOrderId = null, CancellationToken cancellationToken = default);
}
