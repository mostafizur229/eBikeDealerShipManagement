namespace eBike.Domain.Entities;

public class Employee
{
    public int EmployeeID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FName { get; set; }
    public string? MName { get; set; }
    public string? ContactNo { get; set; }
    public string? EmailID { get; set; }
    public string? NID { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string? PresentAdd { get; set; }
    public string? PermanentAdd { get; set; }
    public int DesignationID { get; set; }
    public decimal GrossSalary { get; set; }
    public string? PhotoPath { get; set; }
    public DateTime? DOB { get; set; }
    public decimal SRDueLimit { get; set; }
    public int ConcernID { get; set; }
    public int? DepartmentID { get; set; }
    public int? LocationID { get; set; }
    public int? ReligionID { get; set; }
    public int? GradeID { get; set; }
    public decimal BasicSalary { get; set; }
    public int? PaymentMode { get; set; }
    public int? IsEligibleOT { get; set; }
    public DateTime? EndOfContractDate { get; set; }
    public DateTime? DateOfConfirmation { get; set; }
    public int MachineEMPID { get; set; }
    public int Status { get; set; } = 1;
    public Designation? Designation { get; set; }
    public Department? Department { get; set; }
    public Religion? Religion { get; set; }
    public SisterConcern? SisterConcern { get; set; }
}
