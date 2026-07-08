namespace HRBars.Domain.Entities;

public class Application
{
    public Guid Id { get; set; }
    public short Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    public Guid CandidateId { get; set; }
    public Candidate Candidate { get; set; } = null!;
    
    public Guid VacancyId { get; set; }
    public Vacancy Vacancy { get; set; } = null!;
    
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    public Guid? ArchivedByUserId { get; set; }
    public User? ArchivedByUser { get; set; }

    public ICollection<ApplicationStatusHistory> StatusHistories { get; set; } = new List<ApplicationStatusHistory>();
    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
}