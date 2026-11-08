using eBike.Domain.Enums;

namespace eBike.Application.Features.EBikes.DTOs;

public class EBikeDto
{
    public int Id { get; set; }
    public string ChassisNumber { get; set; } = string.Empty;
    public string MotorNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string BatteryCapacity { get; set; } = string.Empty;
    public string MotorPower { get; set; } = string.Empty;
    public decimal PurchaseCost { get; set; }
    public decimal SellingPrice { get; set; }
    public EBikeStatus Status { get; set; }
    public int? CurrentOdometerKm { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
