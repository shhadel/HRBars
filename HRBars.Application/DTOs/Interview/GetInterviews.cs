using HRBars.Domain.Enums;

namespace HRBars.Application.DTOs.Interview;

public class GetInterviews
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public Guid? CandidateId { get; set; }

    public Guid? VacancyId { get; set; }

    public InterviewStatus? Status { get; set; }

    public InterviewResult? Result { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}