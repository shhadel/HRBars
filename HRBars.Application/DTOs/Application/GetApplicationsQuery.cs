namespace HRBars.Application.DTOs.Application;

public class GetApplicationsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CandidateId { get; set; }
    public Guid? VacancyId { get; set; }
    public ApplicationStatus? Status { get; set; }
}