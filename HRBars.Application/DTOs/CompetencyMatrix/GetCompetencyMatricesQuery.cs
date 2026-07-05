namespace HRBars.Application.DTOs.CompetencyMatrix;

public class GetCompetencyMatricesQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? VacancyId { get; set; }
    public string? Search { get; set; }
    public bool? IsArchived { get; set; }
}