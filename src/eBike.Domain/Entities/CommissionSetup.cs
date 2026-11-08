namespace eBike.Domain.Entities;

public class CommissionSetup
{
    public int CSID { get; set; }
    public int EmployeeID { get; set; }
    public decimal AchievedPercentStart { get; set; }
    public decimal AchievedPercentEnd { get; set; }
    public DateTime CommissionMonth { get; set; }
    public decimal CommisssionAmt { get; set; }
    public decimal CommissionPercent { get; set; }
    public int ConcernID { get; set; }
    public int Status { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Employee? Employee { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}

public class DesWiseCommission
{
    public int ID { get; set; }
    public decimal CommissionPercent { get; set; }
    public int DesignationID { get; set; }
    public int ConcernID { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public Designation? Designation { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
