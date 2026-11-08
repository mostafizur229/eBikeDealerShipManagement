using eBike.Domain.Enums;

namespace eBike.Application.Features.Services.DTOs;

public class ServiceTicketDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int? EBikeId { get; set; }
    public string? EBikeChassisNumber { get; set; }
    public string? EBikeModel { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? NationalId { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public ServiceStatus Status { get; set; }
    public ServicePriority Priority { get; set; }
    public int CurrentMileageKm { get; set; }
    public string IssueDescription { get; set; } = string.Empty;
    public string? DiagnosisReport { get; set; }
    public string? WorkNotes { get; set; }
    public string? BayName { get; set; }
    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PromisedDeliveryDate { get; set; }

    // Linked Parts Sales Order & Stock Deduction tracking
    public int? SalesOrderId { get; set; }
    public string? SalesInvoiceNo { get; set; }
    public bool StockDeducted => SalesOrderId.HasValue && SalesOrderId.Value > 0;

    public bool IsChecklistCompleted { get; set; }
    public int? ChecklistId { get; set; }

    public List<ServiceTicketItemDto> UsedParts { get; set; } = new();
}

public class ServiceTicketItemDto
{
    public int Id { get; set; }
    public int SparePartId { get; set; }
    public string? PartCode { get; set; }
    public string? PartName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
