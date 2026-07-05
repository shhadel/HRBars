namespace HRBars.Application.DTOs.Competency;

public class CompetencyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public Guid TemplateId { get; set; }
}