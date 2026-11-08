namespace eBike.Application.Features.Thanas.DTOs;

public class ThanaDto
{
    public int ThanaID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DistrictID { get; set; }
    public string? DistrictName { get; set; }
}

public class ThanaListDto
{
    public int ThanaID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DistrictID { get; set; }
    public string? DistrictName { get; set; }
}
