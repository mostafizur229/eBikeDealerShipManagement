namespace eBike.Domain.Entities;

public class TransferDetail
{
    public int TDetailID { get; set; }
    public int ProductID { get; set; }
    public int ToProductID { get; set; }
    public int ToGodownID { get; set; }
    public int ToColorID { get; set; }
    public decimal PRate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UTAmount { get; set; }
    public int TransferID { get; set; }
    public int SDetailID { get; set; }
    public string? IMEI { get; set; }

    public Transfer? Transfer { get; set; }
    public Product? Product { get; set; }
    public Godown? ToGodown { get; set; }
    public Color? ToColor { get; set; }
}
