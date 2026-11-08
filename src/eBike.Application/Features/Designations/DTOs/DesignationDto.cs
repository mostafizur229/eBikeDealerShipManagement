namespace eBike.Application.Features.Designations.DTOs;

public class DesignationDto
{
    public int DesignationID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? CommissionPercent { get; set; }
}

public class DesignationListDto
{
    public int DesignationID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? CommissionPercent { get; set; }
}
