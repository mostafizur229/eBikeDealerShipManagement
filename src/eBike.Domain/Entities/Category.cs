namespace eBike.Domain.Entities;

public class Category
{
    public int CategoryID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ConcernID { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
