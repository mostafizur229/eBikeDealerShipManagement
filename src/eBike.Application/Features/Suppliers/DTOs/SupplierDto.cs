namespace eBike.Application.Features.Suppliers.DTOs;

public class SupplierDto
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
    public string? ConcernName { get; set; }
    public decimal? OpeningDue { get; set; }
    public string? Remarks { get; set; }
}

public class SupplierListDto
{
    public int SupplierID { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? OwnerName { get; set; }
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public decimal TotalDue { get; set; }
    public int ConcernID { get; set; }
    public string? ConcernName { get; set; }
    public decimal? OpeningDue { get; set; }
}
