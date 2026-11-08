using System;
using System.Collections.Generic;

namespace eBike.Domain.Entities;

public class JobCard
{
    public int JobCardID { get; set; }
    public string JobCardNo { get; set; } = string.Empty;
    public DateTime JobCardDate { get; set; } = DateTime.UtcNow.Date;
    public int? BookingID { get; set; }
    public int CustomerID { get; set; }
    public Customer Customer { get; set; } = null!;
    public int VehicleID { get; set; }
    public CustomerVehicle? Vehicle { get; set; }
    public int GodownID { get; set; }
    public int? BayID { get; set; }

    public decimal OdometerReading { get; set; }
    public int? FuelLevelPercentage { get; set; }
    public string ServiceType { get; set; } = "FreeService"; // FreeService, PaidService, RunningRepair, AccidentalRepair
    public int? FreeServiceNumber { get; set; }
    public bool IsWaitAndTake { get; set; }
    public bool IsRepeatComplaint { get; set; }
    public int? RevisitJobCardID { get; set; }

    public string CustomerVoiceDescription { get; set; } = string.Empty;
    public string? ServiceAdvisorNotes { get; set; }

    public DateTime? JobStartDateTime { get; set; }
    public DateTime? JobEndDateTime { get; set; }
    public DateTime? EstimatedDeliveryDateTime { get; set; }
    public DateTime? ActualDeliveryDateTime { get; set; }

    public int ServiceAdvisorEmployeeID { get; set; }
    public int? QualityInspectorEmployeeID { get; set; }

    public decimal PartsTotalAmount { get; set; }
    public decimal LaborTotalAmount { get; set; }
    public decimal SubTotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VATAmount { get; set; }
    public decimal GrandTotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }

    public string Status { get; set; } = "Open"; // Open, InWorkshop, QualityCheck, Invoiced, Delivered
    public int ConcernID { get; set; } = 3056;
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public ICollection<JobCardLaborDetail> LaborDetails { get; set; } = new List<JobCardLaborDetail>();
    public ICollection<JobCardPartsDetail> PartsDetails { get; set; } = new List<JobCardPartsDetail>();
}

public class JobCardLaborDetail
{
    public int LaborDetailID { get; set; }
    public int JobCardID { get; set; }
    public JobCard JobCard { get; set; } = null!;
    public string WorkDescription { get; set; } = string.Empty;
    public int TechnicianEmployeeID { get; set; }
    public int Unit { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsFreeLabor { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? ProblemSolvedDescription { get; set; }
    public string WorkStatus { get; set; } = "Completed";
}

public class JobCardPartsDetail
{
    public int PartsDetailID { get; set; }
    public int JobCardID { get; set; }
    public JobCard JobCard { get; set; } = null!;
    public int ProductID { get; set; }
    public int? PartsNoID { get; set; }
    public string? PartCode { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int IssuedGodownID { get; set; }
    public decimal Quantity { get; set; } = 1.00m;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsFreePart { get; set; }
    public bool WarrantyReplaced { get; set; }
    public bool StockDeducted { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
