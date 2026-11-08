namespace eBike.Application.Features.Religions.DTOs;

public class ReligionDto
{
    public int ReligionID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SequenceNO { get; set; }
    public int Status { get; set; }
}

public class ReligionListDto
{
    public int ReligionID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SequenceNO { get; set; }
    public int Status { get; set; }
}
