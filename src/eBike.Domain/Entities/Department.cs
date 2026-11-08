namespace eBike.Domain.Entities;

public class Department
{
    public int DepartmentId { get; set; }
    public string CODE { get; set; } = string.Empty;
    public string DESCRIPTION { get; set; } = string.Empty;
    public int SequenceNO { get; set; }
    public int Status { get; set; } = 1;
    public int CreatedBy { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
