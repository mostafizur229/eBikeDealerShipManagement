namespace eBike.Domain.Entities;

public class Thana
{
    public int ThanaID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DistrictID { get; set; }

    public District? District { get; set; }
}
