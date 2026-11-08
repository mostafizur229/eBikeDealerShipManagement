namespace eBike.Domain.Entities;

public class POrderDetail
{
    public int POrderDetailID { get; set; }
    public int ProductID { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TAmount { get; set; }
    public int POrderID { get; set; }
    public decimal PPDISPer { get; set; }
    public decimal PPDISAmt { get; set; }
    public decimal MRPRate { get; set; }
    public int? ColorID { get; set; }
    public decimal SalesRate { get; set; }
    public decimal ExtraPPDISPer { get; set; }
    public decimal ExtraPPDISAmt { get; set; }
    public decimal PPOFFER { get; set; }
    public decimal CreditSalesRate { get; set; }
    public decimal CRSalesRate12Month { get; set; }
    public decimal CRSalesRate3Month { get; set; }
    public int GodownID { get; set; }
    public decimal Mushok { get; set; }
    public decimal MRPWMushok { get; set; }
    public decimal DPRate { get; set; }
    public decimal RecQuantity { get; set; }
    public int DOrderDetailId { get; set; }
    public decimal PWTaxAmt { get; set; }

    public POrder? POrder { get; set; }
    public Product? Product { get; set; }
    public Color? Color { get; set; }
    public Godown? Godown { get; set; }
    public ICollection<POProductDetail> POProductDetails { get; set; } = new List<POProductDetail>();
}
