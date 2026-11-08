namespace eBike.Domain.Entities;

public class HolidayCalender
{
    public int ID { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public int Type { get; set; } // 1: WeeklyHoliday, 2: SpecialHoliday
    public int Status { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int ConcernID { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
