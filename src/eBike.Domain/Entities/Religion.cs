namespace eBike.Domain.Entities;

public class Religion
{
    public int ReligionID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SequenceNO { get; set; }
    public int Status { get; set; } = 1;
    public int? CreatedBy { get; set; }
    public DateTime? CreateDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
