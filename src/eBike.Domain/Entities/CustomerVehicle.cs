using System;
using System.Collections.Generic;

namespace eBike.Domain.Entities;

public class CustomerVehicle
{
    public int VehicleID { get; set; }
    public int CustomerID { get; set; }
    public Customer Customer { get; set; } = null!;

    public int ProductID { get; set; }
    public Product Product { get; set; } = null!;

    public int? ColorID { get; set; }
    public Color? Color { get; set; }

    public string? RegistrationNo { get; set; }
    public string ChassisNo { get; set; } = string.Empty;
    public string EngineNo { get; set; } = string.Empty;
    public string? KeyNo { get; set; }

    public string OriginType { get; set; } = "OwnShowroom"; // "OwnShowroom" vs "ExternalShowroom"
    public int? SOrderID { get; set; }
    public SOrder? SOrder { get; set; }

    public string? OriginalShowroomName { get; set; }
    public string? OriginalInvoiceNo { get; set; }
    public DateTime? OriginalPurchaseDate { get; set; }
    public string? RelocationReasonNotes { get; set; }

    public string WarrantyStatus { get; set; } = "Active"; // "Active", "Expired", "Void"
    public DateTime? WarrantyExpiryDate { get; set; }
    public int TotalFreeServicesEntitled { get; set; } = 4;
    public int FreeServicesCompleted { get; set; } = 0;
    public decimal CurrentOdometerReading { get; set; } = 0.00m;
    public DateTime? LastServiceDate { get; set; }

    public int ConcernID { get; set; } = 3056;
    public SisterConcern? SisterConcern { get; set; }

    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public ICollection<JobCard> JobCards { get; set; } = new List<JobCard>();
}
