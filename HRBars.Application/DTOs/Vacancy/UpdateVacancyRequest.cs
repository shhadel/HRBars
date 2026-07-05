using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Vacancy;
public class UpdateVacancyRequest
{
    [Required(ErrorMessage = "Название вакансии обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Title { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Название отдела не должно превышать 100 символов")]
    public string? Department { get; set; }

    [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
    public string? Description { get; set; }
}