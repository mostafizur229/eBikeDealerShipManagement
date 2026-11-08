namespace eBike.Domain.Entities;

public class Part
{
    public int PartsNoID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ConcernID { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
