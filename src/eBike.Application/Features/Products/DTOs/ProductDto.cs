using eBike.Domain.Enums;

namespace eBike.Application.Features.Products.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? PicturePath { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }

    public decimal PWDiscount { get; set; }
    public DateTime? DisDurationFDate { get; set; }
    public DateTime? DisDurationToDate { get; set; }
    public EnumUnitType? UnitType { get; set; }
    public EnumProductType ProductType { get; set; }

    // Warranty specs
    public string? CompressorWarrentyMonth { get; set; }
    public string? PanelWarrentyMonth { get; set; }
    public string? MotorWarrentyMonth { get; set; }
    public string? SparePartsWarrentyMonth { get; set; }
    public string? ServiceWarrentyMonth { get; set; }

    // Pricing & Tax
    public decimal? MRP { get; set; }
    public decimal VatAmount { get; set; }
    public decimal PMRate { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal PMushokRate { get; set; }

    // Vehicle & Physical specs
    public string? Weight { get; set; }
    public string? SeatingCapacity { get; set; }
    public string? TiyerSize { get; set; }
    public string? RingSize { get; set; }
    public string? NoofCylinder { get; set; }
    public string? VehicleSize { get; set; }
    public string? HorsePower { get; set; }
    public string? RPM { get; set; }
    public string? CC { get; set; }
    public string? UnLadenWeight { get; set; }
    public string? MakersName { get; set; }
    public string? MFGYear { get; set; }
    public string? ApplicableModel { get; set; }
    public string? PartsName { get; set; }
    public string? MadeIn { get; set; }
    public int IsPartsService { get; set; }
    public int PartsNoId { get; set; }
    public string? PartNo { get; set; }
    public int PLocationId { get; set; }
    public string? LocationName { get; set; }
}
