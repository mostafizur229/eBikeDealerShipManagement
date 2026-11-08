namespace eBike.Domain.Entities;

public class TargetSetup
{
    public int TID { get; set; }
    public int EmployeeID { get; set; }
    public DateTime TargetMonth { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public int ConcernID { get; set; }
    public int Status { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Employee? Employee { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<TargetSetupDetail> TargetSetupDetails { get; set; } = new List<TargetSetupDetail>();
}

public class TargetSetupDetail
{
    public int ID { get; set; }
    public int ProductID { get; set; }
    public int TID { get; set; }
    public decimal Quantity { get; set; }

    public TargetSetup? TargetSetup { get; set; }
    public Product? Product { get; set; }
}
