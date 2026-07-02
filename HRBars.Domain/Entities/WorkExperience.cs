namespace HRBars.Domain.Entities;

public class WorkExperience
{
    public Guid Id { get; set; }
    public string Company { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}