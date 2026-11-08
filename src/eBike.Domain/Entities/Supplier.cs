namespace eBike.Domain.Entities;

public class Supplier
{
    public int SupplierID { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? OwnerName { get; set; }
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? PhotoPath { get; set; }
    public decimal TotalDue { get; set; }
    public int ConcernID { get; set; }
    public decimal? OpeningDue { get; set; }
    public string? Remarks { get; set; }

    public SisterConcern? SisterConcern { get; set; }
}
