namespace eBike.Application.Features.Godowns.DTOs;

public class GodownDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
    public bool IsCommon { get; set; }
}
