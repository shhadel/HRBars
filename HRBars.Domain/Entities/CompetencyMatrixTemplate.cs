namespace HRBars.Domain.Entities;

public class CompetencyMatrixTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    
    public Guid? VacancyId { get; set; }
    public Vacancy? Vacancy { get; set; }
    
    public ICollection<Competency> Competencies { get; set; } = new List<Competency>();
}