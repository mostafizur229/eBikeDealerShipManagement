namespace eBike.Domain.Entities;

public class TransferHistory
{
    public int TransferHID { get; set; }
    public DateTime TransferDate { get; set; }
    public int CreatedBy { get; set; }
    public int FromGodown { get; set; }
    public int ToGodown { get; set; }
    public int ProductId { get; set; }
    public decimal Qty { get; set; }
    public int FromSDetailID { get; set; }
    public int ToSDetailID { get; set; }
    public int POrderDetailID { get; set; }
    public int ConcernID { get; set; }

    public Product? Product { get; set; }
    public Godown? FromGodownNavigation { get; set; }
    public Godown? ToGodownNavigation { get; set; }
}
