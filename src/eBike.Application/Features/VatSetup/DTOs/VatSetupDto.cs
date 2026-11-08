namespace eBike.Application.Features.VatSetup.DTOs;

public class VatSetupDto
{
    public int ConcernID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
    public decimal SalesShowPercent { get; set; }
    public decimal PurchaseShowPercent { get; set; }
    public decimal StockShowPercent { get; set; }
}
