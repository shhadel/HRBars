namespace HRBars.Domain.Entities;

public class InterviewCompetencyScore
{
    public Guid Id { get; set; }
    public short Score { get; set; }
    public short Weight { get; set; } = 1;
    public short MaxScore { get; set; } = 10;
    public string? Comment { get; set; }
    
    public Guid InterviewId { get; set; }
    public Interview Interview { get; set; } = null!;
    
    public Guid CompetencyId { get; set; }
    public Competency Competency { get; set; } = null!;
}