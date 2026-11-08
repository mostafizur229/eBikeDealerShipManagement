namespace eBike.Domain.Entities;

public class PurchaseDODetail
{
    public int DODID { get; set; }
    public int DOID { get; set; }
    public int ProductID { get; set; }
    public int ColorID { get; set; }
    public decimal DOQty { get; set; }
    public decimal GivenQty { get; set; }
    public decimal MRP { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal PPDisPercent { get; set; }
    public decimal PPDisAmt { get; set; }
    public decimal PPTaxAmount { get; set; }

    public PurchaseDO? PurchaseDO { get; set; }
    public Product? Product { get; set; }
    public Color? Color { get; set; }
}
