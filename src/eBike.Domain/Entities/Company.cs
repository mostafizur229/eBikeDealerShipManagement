namespace eBike.Domain.Entities;

public class Company
{
    public int CompanyID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ConcernID { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
