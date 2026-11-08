namespace eBike.Domain.Entities;

public class SOrderDetail
{
    public int SOrderDetailID { get; set; }
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal PPDPercentage { get; set; }
    public decimal PPDAmount { get; set; }
    public decimal Quantity { get; set; }
    public decimal UTAmount { get; set; }
    public int SOrderID { get; set; }
    public decimal MPRate { get; set; }
    public int SDetailID { get; set; }
    public decimal PPOffer { get; set; }
    public decimal SRate { get; set; }
    public decimal PRate { get; set; }
    public int? RStockDetailId { get; set; }
    public int? RepOrderID { get; set; }
    public decimal? RepUnitPrice { get; set; }
    public string? Remarks { get; set; }
    public int IsProductReturn { get; set; }
    public string? Compressor { get; set; }
    public string? Motor { get; set; }
    public string? Panel { get; set; }
    public string? Spareparts { get; set; }
    public string? Service { get; set; }
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
    public int RegType { get; set; }

    public SOrder? SOrder { get; set; }
    public Product? Product { get; set; }
    public StockDetail? StockDetail { get; set; }
}
