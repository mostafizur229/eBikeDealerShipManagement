namespace eBike.Domain.Entities;

public class POrder
{
    public int POrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string ChallanNo { get; set; } = string.Empty;
    public int SupplierID { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal TDiscount { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal RecAmt { get; set; }
    public decimal PaymentDue { get; set; }
    public decimal TotalDue { get; set; }
    public int Status { get; set; }
    public decimal PPDisAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal LaborCost { get; set; }
    public decimal AdjAmount { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int? IsDamageOrder { get; set; }
    public string? Remarks { get; set; }
    public decimal TotalMushok { get; set; }
    public decimal TotalTax { get; set; }
    public int POType { get; set; } = 1;

    public Supplier? Supplier { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<POrderDetail> POrderDetails { get; set; } = new List<POrderDetail>();
}
