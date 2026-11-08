namespace eBike.Application.Features.Parts.DTOs;

public class PartDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
}
