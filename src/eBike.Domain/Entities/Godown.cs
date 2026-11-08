namespace eBike.Domain.Entities;

public class Godown
{
    public int GodownID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int ConcernID { get; set; }
    public int ISCommon { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
