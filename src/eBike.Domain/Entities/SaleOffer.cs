namespace eBike.Domain.Entities;

public class SaleOffer
{
    public int OfferID { get; set; }
    public string? OfferCode { get; set; }
    public int ProductID { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal OfferValue { get; set; }
    public int OfferType { get; set; } // 1: PCS, 2: FlatAmount, 3: TotalValuePer
    public decimal ThresholdValue { get; set; }
    public int? Status { get; set; } // 1: Ongoing, 2: Freeze
    public int ConcernID { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Product? Product { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
