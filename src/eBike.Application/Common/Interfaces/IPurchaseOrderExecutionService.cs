namespace eBike.Application.Common.Interfaces;

public class PurchaseOrderExecutionModel
{
    public DateTime OrderDate { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDiscount { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public decimal AdjAmount { get; set; }
    public int Status { get; set; } = 1;
    public decimal PPTDisAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal LabourCost { get; set; }
    public int ConcernId { get; set; }
    public DateTime CreateDate { get; set; }
    public int CreatedBy { get; set; }
    public int IsDamageOrder { get; set; } = 0;
    public string? Remarks { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalTax { get; set; }
    public int POType { get; set; } = 1;

    public List<PurchaseOrderDetailExecutionItem> Details { get; set; } = new();
}

public class PurchaseOrderDetailExecutionItem
{
    public int? POrderDetailId { get; set; }
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public int? ColorId { get; set; }
    public int Status { get; set; } = 1;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TAmount { get; set; }
    public decimal PPDisPer { get; set; }
    public decimal PPDisAmt { get; set; }
    public decimal MrpRate { get; set; }
    public decimal SalesRate { get; set; }
    public decimal ExtraPPDISPer { get; set; }
    public decimal ExtraPPDISAmt { get; set; }
    public decimal PPOffer { get; set; }
    public decimal CreditSalesRate { get; set; }
    public decimal CRSalesRate12Month { get; set; }
    public decimal CRSalesRate3Month { get; set; }
    public int GodownId { get; set; }
    public decimal Mushok { get; set; }
    public decimal MRPWMushok { get; set; }
    public decimal DPRate { get; set; }
    public decimal RecQuantity { get; set; }
    public int DOrderDetailId { get; set; }
    public decimal PWTaxAmt { get; set; }
    public decimal StockQtyDiff { get; set; }

    public List<POProductDetailExecutionItem> Serials { get; set; } = new();
}

public class POProductDetailExecutionItem
{
    public int ProductId { get; set; }
    public int? ColorId { get; set; }
    public string IMENO { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public int? DamagePOPDID { get; set; }
}

public interface IPurchaseOrderExecutionService
{
    Task<bool> AddPurchaseOrderAsync(PurchaseOrderExecutionModel model, CancellationToken cancellationToken = default);
    Task<bool> UpdatePurchaseOrderAsync(int purchaseOrderId, PurchaseOrderExecutionModel model, CancellationToken cancellationToken = default);
    Task<bool> ReturnPurchaseOrderAsync(int purchaseOrderId, int userId, CancellationToken cancellationToken = default);
    Task<int> CheckProductStatusByPOIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task<int> CheckIMENoDuplicacyByConcernIdAsync(int concernId, string imeNo, CancellationToken cancellationToken = default);
    Task<string> GetNextChallanNoAsync(int concernId, int poType = 1, CancellationToken cancellationToken = default);
}
