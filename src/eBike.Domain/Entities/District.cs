namespace eBike.Domain.Entities;

public class District
{
    public int DistrictID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Thana> Thanas { get; set; } = new List<Thana>();
}
