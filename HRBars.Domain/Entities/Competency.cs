namespace HRBars.Domain.Entities;

public class Competency
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TemplateId { get; set; }
    public CompetencyMatrixTemplate Template { get; set; } = null!;

    public ICollection<InterviewCompetencyScore> InterviewScores { get; set; } = new List<InterviewCompetencyScore>();
}