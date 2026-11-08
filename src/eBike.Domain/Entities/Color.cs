namespace eBike.Domain.Entities;

public class Color
{
    public int ColorID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int ConcernID { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
