namespace eBike.Application.Features.Customers.DTOs;

public class CustomerDto
{
    public int CustomerID { get; set; }
    public string Code { get; set; } = string.Empty; // A/C No.
    public string Name { get; set; } = string.Empty;
    public string? FName { get; set; }
    public string? MName { get; set; }
    public string? HusbandOrWifeName { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactNo { get; set; }
    public string? OthersContactNo { get; set; }
    public string? EmailID { get; set; }
    public string? NID { get; set; }
    public string? Address { get; set; } // Present Address
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
    public int CustomerType { get; set; } // 1: Retail, 2: Dealer, 3: Credit
    public string CustomerTypeName { get; set; } = string.Empty;
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public int ConcernID { get; set; }
    public string? ConcernName { get; set; }
    public string? Remarks { get; set; }
    public string? Village { get; set; }
    public string? Post { get; set; }
    public int SexType { get; set; } // 1: Male, 2: Female
    public string SexTypeName { get; set; } = string.Empty;
    public DateTime? DOB { get; set; }
    public int DistrictID { get; set; }
    public string? DistrictName { get; set; }
    public int ThanaID { get; set; }
    public string? ThanaName { get; set; }
}

public class CustomerListDto
{
    public int CustomerID { get; set; }
    public string Code { get; set; } = string.Empty; // A/C No.
    public string Name { get; set; } = string.Empty;
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public int CustomerType { get; set; }
    public string CustomerTypeName { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal OpeningDue { get; set; }
    public int DistrictID { get; set; }
    public string? DistrictName { get; set; }
    public int ThanaID { get; set; }
    public string? ThanaName { get; set; }
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public int ConcernID { get; set; }
}
