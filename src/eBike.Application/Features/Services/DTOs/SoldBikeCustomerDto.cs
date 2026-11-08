using System;

namespace eBike.Application.Features.Services.DTOs;

public class SoldBikeCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public int ProductId { get; set; }
    public string BikeModel { get; set; } = string.Empty;
    public string ChassisNo { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public int? ColorId { get; set; }
    public DateTime SalesDate { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string? RegistrationNo { get; set; }
    public int? SOrderId { get; set; }
    public int? VehicleId { get; set; }
    public decimal CurrentMileageKm { get; set; }
    public int FreeServicesCompleted { get; set; }
    public int TotalFreeServicesEntitled { get; set; } = 4;
    public string OriginType { get; set; } = "OwnShowroom";
    public string WarrantyStatus { get; set; } = "Active";
}
