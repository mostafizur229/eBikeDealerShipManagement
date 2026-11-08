namespace eBike.Application.Features.SaleOffers.DTOs;

public class SaleOfferDto
{
    public int OfferID { get; set; }
    public string? OfferCode { get; set; }
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal OfferValue { get; set; }
    public int OfferType { get; set; } // 1: PCS, 2: FlatAmount, 3: TotalValuePer
    public string? OfferTypeName { get; set; }
    public decimal ThresholdValue { get; set; }
    public int? Status { get; set; } // 1: Ongoing, 2: Freeze
    public string? StatusName { get; set; }
    public int ConcernID { get; set; }
}

public class SaleOfferListDto
{
    public int OfferID { get; set; }
    public string? OfferCode { get; set; }
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal OfferValue { get; set; }
    public int OfferType { get; set; }
    public string? OfferTypeName { get; set; }
    public decimal ThresholdValue { get; set; }
    public int? Status { get; set; }
    public string? StatusName { get; set; }
}
