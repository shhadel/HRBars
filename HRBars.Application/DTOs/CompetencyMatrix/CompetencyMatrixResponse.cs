using HRBars.Application.DTOs.Competency;

namespace HRBars.Application.DTOs.CompetencyMatrix;
public class CompetencyMatrixResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public Guid? VacancyId { get; set; }
    public string? VacancyTitle { get; set; }
    public List<CompetencyResponse> Competencies { get; set; } = new();
}