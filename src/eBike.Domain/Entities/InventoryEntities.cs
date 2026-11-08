using eBike.Domain.Common;
using eBike.Domain.Enums;

namespace eBike.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<EBikeMaster> EBikes { get; set; } = new List<EBikeMaster>();
}



public class EBikeMaster : BaseEntity
{
    public string ChassisNumber { get; set; } = string.Empty; // Unique VIN
    public string MotorNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string BatteryCapacity { get; set; } = string.Empty; // e.g. "48V 20Ah", "60V 30Ah"
    public string MotorPower { get; set; } = string.Empty; // e.g. "1200W", "1500W"
    public decimal PurchaseCost { get; set; }
    public decimal SellingPrice { get; set; }
    public EBikeStatus Status { get; set; } = EBikeStatus.Available;
    public int? CurrentOdometerKm { get; set; } = 0;
    public string? Barcode { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int? SupplierId { get; set; }
}

public class SparePart : BaseEntity
{
    public string PartCode { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Battery, Controller, Motor, Brake, Tire, etc.
    public string Unit { get; set; } = "Pcs";
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int CurrentStock { get; set; }
    public int MinStockAlertLevel { get; set; } = 5;
    public bool IsActive { get; set; } = true;
}

public class StockMovement : BaseEntity
{
    public StockTransactionType TransactionType { get; set; }
    public int? SparePartId { get; set; }
    public SparePart? SparePart { get; set; }

    public int? EBikeId { get; set; }
    public EBikeMaster? EBike { get; set; }

    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // PO number, Job Card No, etc.
    public string? Remarks { get; set; }
}
