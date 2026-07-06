using HRBars.Domain.Enums;

namespace HRBars.Application.DTOs.Interview;

public class InterviewResponse
{
	public Guid Id { get; set; }

	public DateTime InterviewDate { get; set; }

	public InterviewFormat Format { get; set; }

	public InterviewStatus Status { get; set; }

	public InterviewResult? Result { get; set; }

	public string CandidateName { get; set; } = string.Empty;

	public string VacancyTitle { get; set; } = string.Empty;

	public string CreatedBy { get; set; } = string.Empty;
}