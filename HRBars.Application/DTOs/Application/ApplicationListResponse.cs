namespace HRBars.Application.DTOs.Application;

public class ApplicationListResponse
{
    public Guid Id { get; set; }
    public ApplicationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public string CandidateName { get; set; } = string.Empty;
    public string VacancyTitle { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
}