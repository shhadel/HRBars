using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Application;

public class CreateApplicationRequest
{
    [Required(ErrorMessage = "ID кандидата обязателен")]
    public Guid CandidateId { get; set; }

    [Required(ErrorMessage = "ID вакансии обязателен")]
    public Guid VacancyId { get; set; }

    [Required(ErrorMessage = "Статус обязателен")]
    public ApplicationStatus Status { get; set; }

    public string? Comment { get; set; }
}