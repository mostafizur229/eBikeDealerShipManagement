namespace eBike.Domain.Entities;

public class Designation
{
    public int DesignationID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? CommissionPercent { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
