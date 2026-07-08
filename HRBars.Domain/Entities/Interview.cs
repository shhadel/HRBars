namespace HRBars.Domain.Entities;

public class Interview
{
    public Guid Id { get; set; }
    public DateTime InterviewDate { get; set; }
    public short Format { get; set; }
    public short Status { get; set; }
    public short? DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? Plan { get; set; }
    public short? Result { get; set; }
    public string? DecisionComment { get; set; }
    public DateTime? DecisionDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    
    public Guid? DecidedByUserId { get; set; }
    public User? DecidedByUser { get; set; }

    public Guid? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }

    public Guid? ArchivedByUserId { get; set; }
    public User? ArchivedByUser { get; set; }

    public ICollection<InterviewCompetencyScore> CompetencyScores { get; set; } = new List<InterviewCompetencyScore>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}