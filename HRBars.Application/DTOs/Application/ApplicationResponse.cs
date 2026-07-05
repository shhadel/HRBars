namespace HRBars.Application.DTOs.Application;

public class ApplicationResponse
{
    public Guid Id { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ApplicationCandidateDto Candidate { get; set; } = null!;
    public ApplicationVacancyDto Vacancy { get; set; } = null!;
    public ApplicationUserDto CreatedByUser { get; set; } = null!;

    public List<ApplicationStatusHistoryDto> StatusHistories { get; set; } = new();
    public List<ApplicationInterviewDto> Interviews { get; set; } = new();
}