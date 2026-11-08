namespace eBike.Application.Features.Employees.DTOs;

public class EmployeeDto
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
    public string? DesignationName { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal BasicSalary { get; set; }
    public string? PhotoPath { get; set; }
    public DateTime? DOB { get; set; }
    public decimal SRDueLimit { get; set; }
    public int ConcernID { get; set; }
    public string? ConcernName { get; set; }
    public int? DepartmentID { get; set; }
    public string? DepartmentName { get; set; }
    public int? ReligionID { get; set; }
    public string? ReligionName { get; set; }
    public int MachineEMPID { get; set; }
    public int Status { get; set; }
}

public class EmployeeListDto
{
    public int EmployeeID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? EmailID { get; set; }
    public DateTime? JoiningDate { get; set; }
    public int DesignationID { get; set; }
    public string? DesignationName { get; set; }
    public int? DepartmentID { get; set; }
    public string? DepartmentName { get; set; }
    public decimal GrossSalary { get; set; }
    public int MachineEMPID { get; set; }
    public int Status { get; set; }
    public int ConcernID { get; set; }
    public string? ConcernName { get; set; }
}
