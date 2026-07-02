namespace HRBars.Domain.Entities;

public class Competency
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    
    public Guid TemplateId { get; set; }
    public CompetencyMatrixTemplate Template { get; set; } = null!;
    
    public ICollection<InterviewCompetencyScore> InterviewScores { get; set; } = new List<InterviewCompetencyScore>();
}