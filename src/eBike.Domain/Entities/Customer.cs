namespace eBike.Domain.Entities;

public class Customer
{
    public int CustomerID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FName { get; set; }
    public string? MName { get; set; }
    public string? HusbandOrWifeName { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactNo { get; set; }
    public string? OthersContactNo { get; set; }
    public string? EmailID { get; set; }
    public string? NID { get; set; }
    public string? Address { get; set; }
    public string? PermanentAddress { get; set; }
    public string? PhotoPath { get; set; }
    public decimal OpeningDue { get; set; }
    public decimal TotalDue { get; set; }
    public decimal CreditDue { get; set; }
    public decimal CusDueLimit { get; set; }
    public string? RefName { get; set; }
    public string? RefFName { get; set; }
    public string? RefContact { get; set; }
    public string? RefAddress { get; set; }
    public int CustomerType { get; set; } = 1;
    public int EmployeeID { get; set; }
    public int ConcernID { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public DateTime? RemindDate { get; set; }
    public string? Remarks { get; set; }
    public string? Dist { get; set; }
    public string? Thana { get; set; }
    public string? Post { get; set; }
    public string? Village { get; set; }
    public int SexType { get; set; } = 1;
    public DateTime? DOB { get; set; }
    public int DistrictID { get; set; }
    public int ThanaID { get; set; }
    public int? OldCusID { get; set; }
    public int Id => CustomerID;
    public string Phone => ContactNo ?? string.Empty;
    public District? District { get; set; }
    public Thana? ThanaObj { get; set; }
    public Employee? Employee { get; set; }
    public SisterConcern? SisterConcern { get; set; }
    public ICollection<ServiceTicket> ServiceTickets { get; set; } = new List<ServiceTicket>();
}
