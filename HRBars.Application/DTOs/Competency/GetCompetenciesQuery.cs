namespace HRBars.Application.DTOs.Competency;

public class GetCompetenciesQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? TemplateId { get; set; }
    public string? Category { get; set; }
    public string? Search { get; set; }
    public bool? IsArchived { get; set; }
}