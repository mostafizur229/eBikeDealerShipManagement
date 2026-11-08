namespace eBike.Application.Features.Services.DTOs;

public class WorkshopBayDto
{
    public int BayId { get; set; }
    public string BayName { get; set; } = string.Empty;
    public string BayType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
}

public class WorkshopTimeSlotDto
{
    public int SlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int MaxCapacity { get; set; } = 4;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
}
