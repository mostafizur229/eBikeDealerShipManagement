namespace eBike.Application.Features.CreditSalesOrders.DTOs;

public class CalculatedScheduleItemDto
{
    public int ScheduleNo { get; set; }
    public DateTime MonthDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal NetValue { get; set; }
    public decimal HireValue { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public string PaymentStatus { get; set; } = "Due";
}

public class CalculateInstallmentScheduleResultDto
{
    public decimal TotalAmount { get; set; }
    public decimal DownPayment { get; set; }
    public decimal RemainingAmount { get; set; }
    public int NoOfInstallment { get; set; }
    public decimal MonthlyInstallmentAmount { get; set; }
    public List<CalculatedScheduleItemDto> Schedules { get; set; } = new();
}
