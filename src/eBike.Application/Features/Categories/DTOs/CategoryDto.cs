namespace eBike.Application.Features.Categories.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ConcernId { get; set; }
    public string? ConcernName { get; set; }
}
