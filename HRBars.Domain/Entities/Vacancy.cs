namespace HRBars.Domain.Entities;

public class Vacancy
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Description { get; set; }
    public decimal? SalaryFrom { get; set; }
    public decimal? SalaryTo { get; set; }
    public short ExperienceRequired { get; set; }
    public short EmploymentType { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<CompetencyMatrixTemplate> CompetencyMatrixTemplates { get; set; } = new List<CompetencyMatrixTemplate>();
}