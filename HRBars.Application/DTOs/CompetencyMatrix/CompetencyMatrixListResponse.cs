namespace HRBars.Application.DTOs.CompetencyMatrix;

public class CompetencyMatrixListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public string? VacancyTitle { get; set; }
    public int CompetenciesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}