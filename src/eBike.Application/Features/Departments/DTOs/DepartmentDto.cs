namespace eBike.Application.Features.Departments.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
}

public class DepartmentListDto
{
    public int DepartmentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
}
