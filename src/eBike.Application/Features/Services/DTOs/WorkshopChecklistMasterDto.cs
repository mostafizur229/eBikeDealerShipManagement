using System;

namespace eBike.Application.Features.Services.DTOs;

public class WorkshopChecklistMasterDto
{
    public int ItemId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ToDoAction { get; set; } = "Inspect";
    public string? IntervalNotes { get; set; }
    public string? DefaultRemarks { get; set; }
    public int DisplayOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
