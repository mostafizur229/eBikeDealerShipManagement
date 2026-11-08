namespace eBike.Application.Features.TargetSetups.DTOs;

public class TargetSetupDetailDto
{
    public int ID { get; set; }
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public int TID { get; set; }
    public decimal Quantity { get; set; }
}

public class TargetSetupDto
{
    public int TID { get; set; }
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public DateTime TargetMonth { get; set; }
    public string TargetMonthFormatted => TargetMonth.ToString("MMM yyyy");
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public int ConcernID { get; set; }
    public List<TargetSetupDetailDto> Details { get; set; } = new();
}

public class TargetSetupListDto
{
    public int TID { get; set; }
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public DateTime TargetMonth { get; set; }
    public string TargetMonthFormatted => TargetMonth.ToString("MMM yyyy");
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public int DetailCount { get; set; }
}
