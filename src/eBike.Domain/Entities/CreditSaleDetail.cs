namespace eBike.Domain.Entities;

public class CreditSaleDetail
{
    public int CreditSaleDetailsID { get; set; }
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal UTAmount { get; set; }
    public int CreditSalesID { get; set; }
    public decimal? MPRateTotal { get; set; }
    public decimal MPRate { get; set; }
    public int StockDetailID { get; set; }
    public decimal PPOffer { get; set; }
    public decimal IntPercentage { get; set; }
    public decimal IntTotalAmt { get; set; }
    public string? Compressor { get; set; } // GodownID
    public string? Motor { get; set; }
    public string? Panel { get; set; }
    public string? Spareparts { get; set; }
    public string? Service { get; set; }
    public int IsProductReturn { get; set; }
    public decimal SRate { get; set; }
    public decimal PPVat { get; set; }
    public decimal RegAmt { get; set; }
    public int OfferId { get; set; }
    public decimal OfferAmt { get; set; }
    public decimal Mushok { get; set; }
    public decimal SRateWMushok { get; set; }
    public decimal InsuranceAmt { get; set; }
    public decimal ExtInstAmt { get; set; }
    public int ColorInfoId { get; set; }

    public CreditSale? CreditSale { get; set; }
    public Product? Product { get; set; }
    public StockDetail? StockDetail { get; set; }
}
