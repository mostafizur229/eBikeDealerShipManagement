using eBike.Domain.Common;
using eBike.Domain.Enums;

namespace eBike.Domain.Entities;

public class ServiceTechnician : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty; // e.g., "Electrical & Battery", "Mechanical"
    public bool IsAvailable { get; set; } = true;

    public int? UserId { get; set; }
    public UserMaster? User { get; set; }
}

public class ServiceTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty; // e.g., "SRV-202610-0001"
    
    public int? EBikeId { get; set; }
    public EBikeMaster? EBike { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? AssignedTechnicianId { get; set; }
    public ServiceTechnician? AssignedTechnician { get; set; }

    public ServiceStatus Status { get; set; } = ServiceStatus.Pending;
    public ServicePriority Priority { get; set; } = ServicePriority.Medium;

    public int CurrentMileageKm { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public string? DiagnosisReport { get; set; }
    public string? WorkNotes { get; set; }

    // Costing
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount => (LaborCost + PartsCost) - Discount;
    public decimal PaidAmount { get; set; }

    public DateTime? PromisedDeliveryDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }

    // Linked Parts Sales Order (SOType = 2, inventory deduction)
    public int? SalesOrderId { get; set; }
    public string? SalesInvoiceNo { get; set; }

    public ICollection<ServiceTicketItem> UsedParts { get; set; } = new List<ServiceTicketItem>();
}

public class ServiceTicketItem : BaseEntity
{
    public int ServiceTicketId { get; set; }
    public ServiceTicket ServiceTicket { get; set; } = null!;

    public int SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class WorkshopBay
{
    public int BayId { get; set; }
    public string BayName { get; set; } = string.Empty;
    public string BayType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int ConcernID { get; set; } = 3056;
}

public class WorkshopTimeSlot
{
    public int SlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int MaxCapacity { get; set; } = 4;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int ConcernID { get; set; } = 3056;
}

public class WorkshopChecklistMaster
{
    public int ItemId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ToDoAction { get; set; } = "Inspect";
    public string? IntervalNotes { get; set; }
    public string? DefaultRemarks { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
