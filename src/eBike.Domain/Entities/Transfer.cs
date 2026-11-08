namespace eBike.Domain.Entities;

public class Transfer
{
    public int TransferID { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public int ToConcernID { get; set; }
    public int FromConcernID { get; set; }
    public int Status { get; set; }
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }

    public SisterConcern? FromConcern { get; set; }
    public SisterConcern? ToConcern { get; set; }
    public ICollection<TransferDetail> TransferDetails { get; set; } = new List<TransferDetail>();
}
