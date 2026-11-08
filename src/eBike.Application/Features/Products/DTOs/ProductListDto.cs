namespace eBike.Application.Features.Products.DTOs;

public class ProductListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
    public decimal? MRP { get; set; }
    public decimal PMRate { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal PWDiscount { get; set; }
    public string? CC { get; set; }
    public string? MFGYear { get; set; }
    public string? MadeIn { get; set; }
    public string? PicturePath { get; set; }
    public int IsPartsService { get; set; }
    public string? ApplicableModel { get; set; }
    public int PartsNoId { get; set; }
    public string? PartNo { get; set; }
    public int PLocationId { get; set; }
    public string? LocationName { get; set; }
}
