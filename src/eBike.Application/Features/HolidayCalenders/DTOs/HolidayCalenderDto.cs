namespace eBike.Application.Features.HolidayCalenders.DTOs;

public class HolidayCalenderDto
{
    public int ID { get; set; }
    public DateTime Date { get; set; }
    public string DateFormatted => Date.ToString("dd MMM yyyy");
    public string? Description { get; set; }
    public int Type { get; set; } // 1: WeeklyHoliday, 2: SpecialHoliday
    public string TypeName => Type == 1 ? "Weekly Holiday" : "Special Holiday";
    public int Status { get; set; }
    public int ConcernID { get; set; }
}

public class HolidayMonthDayDto
{
    public DateTime Date { get; set; }
    public string DateFormatted => Date.ToString("dd MMM yyyy");
    public string DayName => Date.ToString("dddd");
    public int DayNumber => Date.Day;
    public bool IsHoliday { get; set; }
    public int? HolidayId { get; set; }
    public int? Type { get; set; }
    public string? Description { get; set; }
}
