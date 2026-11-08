namespace eBike.Application.Features.Colors.DTOs;

public class ColorDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
}
