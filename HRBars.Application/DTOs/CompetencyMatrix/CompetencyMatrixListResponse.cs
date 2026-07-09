namespace HRBars.Application.DTOs.CompetencyMatrix;

public class CompetencyMatrixListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? VacancyId { get; set; }
}