namespace eBike.Application.Features.Registrations.DTOs;

public class RegistrationDocListItemDto
{
    public int DocumentType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ButtonColor { get; set; } = "primary";
    public bool IsMultiPage { get; set; }
    public int PageCount { get; set; } = 1;
    public string TemplateName { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
}

