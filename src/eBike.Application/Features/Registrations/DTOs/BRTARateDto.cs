namespace eBike.Application.Features.Registrations.DTOs;

public class BRTARateDto
{
    public int ID { get; set; }
    public int RegType { get; set; }
    public string? RegTypeName { get; set; }
    public decimal CC { get; set; }
    public decimal NewRegMF { get; set; }
    public decimal NewRegIF { get; set; }
    public decimal IssueTaxTokeMF { get; set; }
    public decimal RegCertificateMF { get; set; }
    public decimal SupplementaryMF { get; set; }
    public decimal NamePlate { get; set; }
    public decimal Contribution { get; set; }
    public decimal TotalAmount { get; set; }
}

