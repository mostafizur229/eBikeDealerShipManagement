namespace eBike.Domain.Entities;

public class Stock
{
    public int StockID { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public decimal Quantity { get; set; }
    public int ProductID { get; set; }
    public decimal MRPPrice { get; set; }
    public decimal LPPrice { get; set; }
    public int? ColorID { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int GodownID { get; set; }

    public Product? Product { get; set; }
    public Color? Color { get; set; }
    public Godown? Godown { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<StockDetail> StockDetails { get; set; } = new List<StockDetail>();
}
