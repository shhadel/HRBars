using System.ComponentModel.DataAnnotations;

namespace HRBars.Application.DTOs.Vacancy
{
    public class CreateVacancy
    {
        [Required(ErrorMessage = "Название вакансии обязательно")]
        [MaxLength(200, ErrorMessage = "Название не может превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Название отдела не может превышать 150 символов")]
        public string? Department { get; set; }

        public string? Description { get; set; }

        public List<string>? CompetencyNames { get; set; }

        public Guid? CompetencyTemplateId { get; set; }
    }
}
