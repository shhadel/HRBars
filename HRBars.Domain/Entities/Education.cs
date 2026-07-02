namespace HRBars.Domain.Entities;

public class Education
{
    public Guid Id { get; set; }
    public string Institution { get; set; } = string.Empty;
    public string? Faculty { get; set; }
    public string? Degree { get; set; }
    public short? StartYear { get; set; }
    public short? EndYear { get; set; }
    
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
}