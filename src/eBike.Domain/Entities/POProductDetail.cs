namespace eBike.Domain.Entities;

public class POProductDetail
{
    public int POPDID { get; set; }
    public int ProductID { get; set; }
    public string IMENO { get; set; } = string.Empty;
    public int POrderDetailID { get; set; }
    public int? ColorID { get; set; }
    public int? DamagePOPDID { get; set; }
    public int? IsDamageReplaced { get; set; }
    public decimal ReturnQty { get; set; }
    public int? ReturnSDetailID { get; set; }
    public string EngineNo { get; set; } = string.Empty;
    public string KeyNo { get; set; } = string.Empty;
    public string? SerialNo { get; set; }

    public POrderDetail? POrderDetail { get; set; }
    public Product? Product { get; set; }
    public Color? Color { get; set; }
}
