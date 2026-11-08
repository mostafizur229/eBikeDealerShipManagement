namespace eBike.Application.Features.CommissionSetups.DTOs;

public class CommissionSetupDto
{
    public int CSID { get; set; }
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public decimal AchievedPercentStart { get; set; }
    public decimal AchievedPercentEnd { get; set; }
    public DateTime CommissionMonth { get; set; }
    public string CommissionMonthFormatted => CommissionMonth.ToString("MMM yyyy");
    public decimal CommisssionAmt { get; set; }
    public decimal CommissionPercent { get; set; }
    public int Status { get; set; }
    public int ConcernID { get; set; }
}

public class CommissionSetupListDto
{
    public int CSID { get; set; }
    public int EmployeeID { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public decimal AchievedPercentStart { get; set; }
    public decimal AchievedPercentEnd { get; set; }
    public DateTime CommissionMonth { get; set; }
    public string CommissionMonthFormatted => CommissionMonth.ToString("MMM yyyy");
    public decimal CommisssionAmt { get; set; }
    public decimal CommissionPercent { get; set; }
    public int Status { get; set; }
}

public class DesWiseCommissionDto
{
    public int ID { get; set; }
    public decimal CommissionPercent { get; set; }
    public int DesignationID { get; set; }
    public string? DesignationName { get; set; }
    public string? DesignationCode { get; set; }
    public int ConcernID { get; set; }
}
