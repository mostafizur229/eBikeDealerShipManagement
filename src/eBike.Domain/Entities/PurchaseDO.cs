namespace eBike.Domain.Entities;

public class PurchaseDO
{
    public int DOID { get; set; }
    public DateTime Date { get; set; }
    public string DONo { get; set; } = string.Empty;
    public int CustomerID { get; set; }
    public int SupplierID { get; set; }
    public int Status { get; set; } = 1; // 1 = DO, 2 = Return, 3 = Complete
    public string? Remarks { get; set; }
    public decimal TotalAmt { get; set; }
    public decimal PaidAmt { get; set; }
    public decimal NetDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Customer? Customer { get; set; }
    public Supplier? Supplier { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<PurchaseDODetail> DODetails { get; set; } = new List<PurchaseDODetail>();
}
