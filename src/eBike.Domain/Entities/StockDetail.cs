namespace eBike.Domain.Entities;

public class StockDetail
{
    public int SDetailID { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public int ProductID { get; set; }
    public string IMENO { get; set; } = string.Empty;
    public int StockID { get; set; }
    public int Status { get; set; }
    public int? ColorID { get; set; }
    public int POrderDetailID { get; set; }
    public decimal PRate { get; set; }
    public decimal SRate { get; set; }
    public decimal CreditSRate { get; set; }
    public decimal CRSalesRate12Month { get; set; }
    public decimal CRSalesRate3Month { get; set; }
    public decimal Quantity { get; set; }
    public int GodownID { get; set; }
    public int TransferDetailID { get; set; }
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;

    public Stock? Stock { get; set; }
    public Product? Product { get; set; }
    public Color? Color { get; set; }
    public POrderDetail? POrderDetail { get; set; }
    public Godown? Godown { get; set; }
}
