using eBike.Domain.Enums;

namespace eBike.Domain.Entities;

public class Product
{
    public int ProductID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? PicturePath { get; set; }
    public int CategoryID { get; set; }
    public int CompanyID { get; set; }
    public int ConcernID { get; set; }
    public decimal PWDiscount { get; set; } = 0;
    public DateTime? DisDurationFDate { get; set; }
    public DateTime? DisDurationToDate { get; set; }
    public EnumUnitType? UnitType { get; set; } = EnumUnitType.Piece;
    public EnumProductType ProductType { get; set; } = EnumProductType.ExistingBC;
    public string? CompressorWarrentyMonth { get; set; }
    public string? PanelWarrentyMonth { get; set; }
    public string? MotorWarrentyMonth { get; set; }
    public string? SparePartsWarrentyMonth { get; set; }
    public string? ServiceWarrentyMonth { get; set; }
    public decimal? MRP { get; set; }
    public decimal VatAmount { get; set; } = 0;
    public decimal PMRate { get; set; } = 0;
    public decimal CommissionRate { get; set; } = 0;
    public decimal PMushokRate { get; set; } = 0;
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
    public int PartsNoID { get; set; } = 0;
    public int PLocationID { get; set; } = 0;
    public int IsPartsService { get; set; } = 0;
    public Category? Category { get; set; }
    public Company? Company { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
