namespace eBike.Application.Features.Districts.DTOs;

public class DistrictDto
{
    public int DistrictID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ThanaCount { get; set; }
}

public class DistrictListDto
{
    public int DistrictID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ThanaCount { get; set; }
}
